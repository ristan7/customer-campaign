using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Domain.Entities;
using CustomerCampaign.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Persistence
{
    public class DbInitializer(
        AppDbContext db,
        IPasswordHasher hasher,
        IConfiguration configuration,
        ILogger<DbInitializer> logger)
    {
        public async Task InitializeAsync(CancellationToken ct = default)
        {
            var provider = configuration.GetValue("DatabaseProvider", DatabaseProvider.MySql);

            if (provider == DatabaseProvider.MySql)
                await db.Database.MigrateAsync(ct);
            else
                await db.Database.EnsureCreatedAsync(ct);

            await SeedAsync(ct);
        }

        private async Task SeedAsync(CancellationToken ct)
        {
            if (!await db.Users.AnyAsync(ct))
            {
                db.Users.AddRange(
                    new User { Username = "admin", FullName = "Campaign Admin", Role = UserRole.Admin, PasswordHash = hasher.Hash("Admin123!") },
                    new User { Username = "agent1", FullName = "Ana Agentić", Role = UserRole.Agent, PasswordHash = hasher.Hash("Agent123!") },
                    new User { Username = "agent2", FullName = "Marko Marković", Role = UserRole.Agent, PasswordHash = hasher.Hash("Agent123!") });
            }

            if (!await db.Campaigns.AnyAsync(ct))
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                db.Campaigns.Add(new Campaign
                {
                    Name = "Loyal Customer Discount 2026",
                    StartDate = today,
                    EndDate = today.AddDays(6),
                    DailyLimitPerAgent = 5,
                    DiscountPercent = 15m
                });
            }

            if (!await db.ApiClients.AnyAsync(ct))
            {
                db.ApiClients.Add(new ApiClient
                {
                    ClientId = "crm-system",
                    Name = "External CRM (demo)",
                    ClientSecretHash = hasher.Hash("crm-secret-2026")
                });
            }

            var changes = await db.SaveChangesAsync(ct);
            if (changes > 0)
                logger.LogInformation("Database seeded with {Count} records.", changes);
        }
    }
}
