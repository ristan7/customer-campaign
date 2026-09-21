using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : DbContext(options), IApplicationDbContext
    {
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<User> Users => Set<User>();
        public DbSet<CustomerReward> CustomerRewards => Set<CustomerReward>();
        public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
        public DbSet<ApiClient> ApiClients => Set<ApiClient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
