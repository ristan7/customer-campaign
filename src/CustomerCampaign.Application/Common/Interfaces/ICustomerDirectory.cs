using CustomerCampaign.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface ICustomerDirectory
    {
        Task<CustomerInfo?> FindByIdAsync(int externalId, CancellationToken ct = default);
    }
}
