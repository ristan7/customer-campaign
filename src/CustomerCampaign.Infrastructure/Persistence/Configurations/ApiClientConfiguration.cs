using CustomerCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Persistence.Configurations
{
    public class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
    {
        public void Configure(EntityTypeBuilder<ApiClient> builder)
        {
            builder.ToTable("api_clients");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.ClientId).IsRequired().HasMaxLength(100);
            builder.HasIndex(c => c.ClientId).IsUnique();
            builder.Property(c => c.ClientSecretHash).IsRequired().HasMaxLength(500);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        }
    }
}
