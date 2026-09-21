using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Enums
{
    public static class Roles
    {
        public const string Agent = nameof(UserRole.Agent);
        public const string Admin = nameof(UserRole.Admin);
        public const string Integration = "Integration";
    }
}
