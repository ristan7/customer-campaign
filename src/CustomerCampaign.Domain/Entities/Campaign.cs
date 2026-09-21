using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DailyLimitPerAgent { get; set; } = 5;
        public decimal DiscountPercent { get; set; }

        public ICollection<CustomerReward> Rewards { get; set; } = new List<CustomerReward>();

        public bool IsActiveOn(DateOnly date) => date >= StartDate && date <= EndDate;
    }
}
