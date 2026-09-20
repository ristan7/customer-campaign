using CustomerCampaign.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CustomerCampaign.Application.Rewards
{
    public record CreateRewardRequest(
    [Range(1, int.MaxValue)] int CustomerExternalId,
    [MaxLength(500)] string? Note);

    public record RewardDto(
        int Id,
        int CampaignId,
        int AgentId,
        string AgentName,
        int CustomerExternalId,
        string CustomerName,
        string? CustomerCity,
        DateOnly RewardDate,
        RewardStatus Status,
        bool DiscountOfferAccepted,
        DateOnly? PurchaseDate,
        string? Note,
        DateTime CreatedAtUtc);

    public record AgentDailyRewardsDto(
        DateOnly Date,
        int DailyLimit,
        int Used,
        int Remaining,
        IReadOnlyList<RewardDto> Items);

    public class RewardQuery
    {
        public int? CampaignId { get; set; }
        public bool? Accepted { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
