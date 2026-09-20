using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Campaigns
{
    public class CampaignService(IApplicationDbContext db, IClock clock) : ICampaignService
    {
        public async Task<CampaignResultsDto> GetCurrentResultsAsync(CancellationToken ct = default)
        {
            var today = clock.Today;

            var campaign = await db.Campaigns.AsNoTracking()
                .Where(c => c.StartDate <= today && c.EndDate >= today)
                .FirstOrDefaultAsync(ct)
                ?? await db.Campaigns.AsNoTracking().OrderByDescending(c => c.EndDate).FirstOrDefaultAsync(ct)
                ?? throw new NotFoundException("No campaign exists.");

            return await GetResultsAsync(campaign.Id, ct);
        }

        public async Task<CampaignResultsDto> GetResultsAsync(int campaignId, CancellationToken ct = default)
        {
            var campaign = await db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == campaignId, ct)
                ?? throw new NotFoundException($"Campaign {campaignId} was not found.");

            var rewards = db.CustomerRewards.AsNoTracking().Where(r => r.CampaignId == campaignId);
            var active = rewards.Where(r => r.Status == RewardStatus.Active);

            var totalRewarded = await active.CountAsync(ct);
            var totalAccepted = await active.CountAsync(r => r.DiscountOfferAccepted, ct);
            var cancelled = await rewards.CountAsync(r => r.Status == RewardStatus.Cancelled, ct);

            var perAgent = await active
                .GroupBy(r => new { r.AgentId, r.Agent.FullName })
                .Select(g => new
                {
                    g.Key.AgentId,
                    g.Key.FullName,
                    Rewarded = g.Count(),
                    Accepted = g.Count(r => r.DiscountOfferAccepted)
                })
                .OrderByDescending(x => x.Accepted)
                .ToListAsync(ct);

            var perDay = await active
                .GroupBy(r => r.RewardDate)
                .Select(g => new
                {
                    Date = g.Key,
                    Rewarded = g.Count(),
                    Accepted = g.Count(r => r.DiscountOfferAccepted)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(ct);

            return new CampaignResultsDto(
                campaign.Id, campaign.Name, campaign.StartDate, campaign.EndDate,
                campaign.DailyLimitPerAgent, campaign.DiscountPercent,
                totalRewarded, totalAccepted, Rate(totalAccepted, totalRewarded), cancelled,
                perAgent.Select(a => new AgentPerformanceDto(
                    a.AgentId, a.FullName, a.Rewarded, a.Accepted, Rate(a.Accepted, a.Rewarded))).ToList(),
                perDay.Select(d => new DailyBreakdownDto(d.Date, d.Rewarded, d.Accepted)).ToList());
        }

        private static decimal Rate(int accepted, int total) =>
            total == 0 ? 0 : Math.Round(accepted * 100m / total, 2);
    }
}
