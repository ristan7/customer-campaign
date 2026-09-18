using CustomerCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Campaign> Campaigns { get; }
        DbSet<User> Users { get; }
        DbSet<CustomerReward> CustomerRewards { get; }
        DbSet<ImportBatch> ImportBatches { get; }
        DbSet<ApiClient> ApiClients { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
