using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Common.Models;
using CustomerCampaign.Domain.Entities;
using CustomerCampaign.Domain.Enums;
using CustomerCampaign.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Rewards
{
    public class RewardService(
    IApplicationDbContext db,
    ICustomerDirectory customerDirectory,
    IClock clock,
    ILogger<RewardService> logger) : IRewardService
    {
        public async Task<RewardDto> CreateAsync(int agentId, CreateRewardRequest request, CancellationToken ct = default)
        {
            var today = clock.Today;

            var agent = await db.Users.FirstOrDefaultAsync(
                    u => u.Id == agentId && u.IsActive && u.Role == UserRole.Agent, ct)
                ?? throw new ForbiddenAccessException("Only active agents can register customers.");

            var campaign = await db.Campaigns.FirstOrDefaultAsync(
                    c => c.StartDate <= today && c.EndDate >= today, ct)
                ?? throw new DomainException($"There is no active campaign on {today:yyyy-MM-dd}.");

            var usedToday = await db.CustomerRewards.CountAsync(r =>
                r.CampaignId == campaign.Id &&
                r.AgentId == agentId &&
                r.RewardDate == today &&
                r.Status == RewardStatus.Active, ct);

            if (usedToday >= campaign.DailyLimitPerAgent)
                throw new DomainException(
                    $"Daily limit of {campaign.DailyLimitPerAgent} customers has been reached for {today:yyyy-MM-dd}.");

            var existing = await db.CustomerRewards.FirstOrDefaultAsync(r =>
                r.CampaignId == campaign.Id && r.CustomerExternalId == request.CustomerExternalId, ct);

            if (existing is { Status: RewardStatus.Active })
                throw new DomainException(
                    $"Customer {request.CustomerExternalId} has already been rewarded in this campaign.");

            var customer = await customerDirectory.FindByIdAsync(request.CustomerExternalId, ct)
                ?? throw new DomainException($"Customer {request.CustomerExternalId} does not exist in CRM.");

            CustomerReward reward;
            if (existing is not null)
            {
                existing.Reactivate(agentId, today, customer.FullName, customer.City, request.Note);
                reward = existing;
            }
            else
            {
                reward = new CustomerReward
                {
                    CampaignId = campaign.Id,
                    AgentId = agentId,
                    CustomerExternalId = request.CustomerExternalId,
                    CustomerName = customer.FullName,
                    CustomerCity = customer.City,
                    RewardDate = today,
                    Note = request.Note
                };
                db.CustomerRewards.Add(reward);
            }

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                logger.LogWarning(ex, "Concurrent reward for customer {CustomerId}", request.CustomerExternalId);
                throw new DomainException(
                    $"Customer {request.CustomerExternalId} has already been rewarded in this campaign.");
            }

            logger.LogInformation("Agent {AgentId} rewarded customer {CustomerId} in campaign {CampaignId}",
                agentId, request.CustomerExternalId, campaign.Id);

            return ToDto(reward, agent.FullName);
        }

        public async Task<AgentDailyRewardsDto> GetMyRewardsAsync(int agentId, DateOnly? date, CancellationToken ct = default)
        {
            var day = date ?? clock.Today;

            var campaign = await db.Campaigns.AsNoTracking()
                .FirstOrDefaultAsync(c => c.StartDate <= day && c.EndDate >= day, ct);

            var items = await Project(db.CustomerRewards.AsNoTracking()
                    .Where(r => r.AgentId == agentId && r.RewardDate == day)
                    .OrderByDescending(r => r.CreatedAtUtc))
                .ToListAsync(ct);

            var limit = campaign?.DailyLimitPerAgent ?? 0;
            var used = items.Count(i => i.Status == RewardStatus.Active);

            return new AgentDailyRewardsDto(day, limit, used, Math.Max(0, limit - used), items);
        }

        public async Task CancelAsync(int agentId, int rewardId, CancellationToken ct = default)
        {
            var reward = await db.CustomerRewards.FirstOrDefaultAsync(r => r.Id == rewardId, ct)
                ?? throw new NotFoundException($"Reward {rewardId} was not found.");

            if (reward.AgentId != agentId)
                throw new ForbiddenAccessException("Agents can cancel only their own rewards.");

            if (reward.RewardDate != clock.Today)
                throw new DomainException("Rewards can be cancelled only on the day they were registered.");

            reward.Cancel(clock.UtcNow);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Agent {AgentId} cancelled reward {RewardId}", agentId, rewardId);
        }

        public async Task<PagedResult<RewardDto>> GetRewardsAsync(RewardQuery query, CancellationToken ct = default)
        {
            var q = db.CustomerRewards.AsNoTracking().Where(r => r.Status == RewardStatus.Active);

            if (query.CampaignId is int campaignId)
                q = q.Where(r => r.CampaignId == campaignId);
            if (query.Accepted is bool accepted)
                q = q.Where(r => r.DiscountOfferAccepted == accepted);

            var page = Math.Max(1, query.Page);
            var size = Math.Clamp(query.PageSize, 1, 100);

            var total = await q.CountAsync(ct);
            var items = await Project(q.OrderBy(r => r.Id).Skip((page - 1) * size).Take(size)).ToListAsync(ct);

            return new PagedResult<RewardDto>(items, page, size, total);
        }

        private static IQueryable<RewardDto> Project(IQueryable<CustomerReward> query) =>
            query.Select(r => new RewardDto(
                r.Id, r.CampaignId, r.AgentId, r.Agent.FullName,
                r.CustomerExternalId, r.CustomerName, r.CustomerCity,
                r.RewardDate, r.Status, r.DiscountOfferAccepted,
                r.PurchaseDate, r.Note, r.CreatedAtUtc));

        private static RewardDto ToDto(CustomerReward r, string agentName) => new(
            r.Id, r.CampaignId, r.AgentId, agentName,
            r.CustomerExternalId, r.CustomerName, r.CustomerCity,
            r.RewardDate, r.Status, r.DiscountOfferAccepted,
            r.PurchaseDate, r.Note, r.CreatedAtUtc);
    }
}
