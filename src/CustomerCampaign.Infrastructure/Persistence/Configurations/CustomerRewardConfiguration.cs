using CustomerCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Persistence.Configurations
{
    public class CustomerRewardConfiguration : IEntityTypeConfiguration<CustomerReward>
    {
        public void Configure(EntityTypeBuilder<CustomerReward> builder)
        {
            builder.ToTable("customer_rewards");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.CustomerName).IsRequired().HasMaxLength(200);
            builder.Property(r => r.CustomerCity).HasMaxLength(100);
            builder.Property(r => r.Note).HasMaxLength(500);
            builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(r => new { r.CampaignId, r.CustomerExternalId }).IsUnique();
            builder.HasIndex(r => new { r.AgentId, r.RewardDate });

            builder.HasOne(r => r.Campaign)
                .WithMany(c => c.Rewards)
                .HasForeignKey(r => r.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Agent)
                .WithMany(u => u.Rewards)
                .HasForeignKey(r => r.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.ImportBatch)
                .WithMany()
                .HasForeignKey(r => r.ImportBatchId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
