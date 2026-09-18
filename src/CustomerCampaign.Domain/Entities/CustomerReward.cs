using CustomerCampaign.Domain.Enums;
using CustomerCampaign.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Entities
{
    public class CustomerReward
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;
        public int AgentId { get; set; }
        public User Agent { get; set; } = null!;
        public int CustomerExternalId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerCity { get; set; }

        public DateOnly RewardDate { get; set; }
        public RewardStatus Status { get; set; } = RewardStatus.Active;
        public string? Note { get; set; }

        public bool DiscountOfferAccepted { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAtUtc { get; set; }

        public void Cancel(DateTime utcNow)
        {
            if (Status == RewardStatus.Cancelled)
                throw new DomainException("Reward is already cancelled.");
            if (DiscountOfferAccepted)
                throw new DomainException("Reward with an accepted offer cannot be cancelled.");

            Status = RewardStatus.Cancelled;
            CancelledAtUtc = utcNow;
        }

        public void Reactivate(int agentId, DateOnly rewardDate, string customerName, string? city, string? note)
        {
            if (Status != RewardStatus.Cancelled)
                throw new DomainException("Only cancelled rewards can be reactivated.");

            AgentId = agentId;
            RewardDate = rewardDate;
            CustomerName = customerName;
            CustomerCity = city;
            Note = note;
            Status = RewardStatus.Active;
            CancelledAtUtc = null;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void MarkPurchased(DateOnly purchaseDate, int importBatchId)
        {
            if (Status == RewardStatus.Cancelled)
                throw new DomainException("Cancelled reward cannot be marked as purchased.");

            DiscountOfferAccepted = true;
            PurchaseDate = purchaseDate;
            ImportBatchId = importBatchId;
        }
    }
}
