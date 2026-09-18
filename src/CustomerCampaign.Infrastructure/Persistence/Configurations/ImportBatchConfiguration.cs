using CustomerCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Persistence.Configurations
{
    public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
    {
        public void Configure(EntityTypeBuilder<ImportBatch> builder)
        {
            builder.ToTable("import_batches");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.FileName).IsRequired().HasMaxLength(255);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(b => b.ImportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
