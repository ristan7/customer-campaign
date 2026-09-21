using CustomerCampaign.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<CustomerReward> Rewards { get; set; } = new List<CustomerReward>();
    }
}
