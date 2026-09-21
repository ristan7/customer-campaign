using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Common.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.ExternalServices.FindPerson
{
    public class StubCustomerDirectory(ILogger<StubCustomerDirectory> logger) : ICustomerDirectory
    {
        private static readonly string[] Names =
            ["Dave R. Newton", "Ralph A. Diavolo", "Emma T. Quixote", "John P. Kearney", "Maria L. Ferraro"];

        private static readonly string[] Cities = ["Pueblo", "Hialeah", "Albany", "Boston", "Denver"];

        public Task<CustomerInfo?> FindByIdAsync(int externalId, CancellationToken ct = default)
        {
            logger.LogWarning("Using STUB customer directory for customer {CustomerId}.", externalId);

            if (externalId is < 1 or > 200)
                return Task.FromResult<CustomerInfo?>(null);

            var dob = new DateOnly(1950 + externalId % 50, 1 + externalId % 12, 1 + externalId % 28);
            var info = new CustomerInfo(
                externalId,
                $"{Names[externalId % Names.Length]} ({externalId})",
                dob,
                DateTime.UtcNow.Year - dob.Year,
                Cities[externalId % Cities.Length],
                "NY");

            return Task.FromResult<CustomerInfo?>(info);
        }
    }
}
