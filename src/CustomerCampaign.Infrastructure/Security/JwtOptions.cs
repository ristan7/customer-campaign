using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = "customer-campaign-api";
        public string Audience { get; set; } = "customer-campaign";
        public string SigningKey { get; set; } = string.Empty;
        public int AccessTokenMinutes { get; set; } = 60;
    }
}
