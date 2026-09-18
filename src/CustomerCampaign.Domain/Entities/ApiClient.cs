using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Entities
{
    public class ApiClient
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecretHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
