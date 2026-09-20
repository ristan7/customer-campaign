using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Domain.Entities;
using CustomerCampaign.Domain.Enums;
using CustomerCampaign.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Imports
{
    public class PurchaseImportService(
    IApplicationDbContext db,
    IPurchaseCsvParser parser,
    ILogger<PurchaseImportService> logger) : IPurchaseImportService
    {
        public async Task<ImportResultDto> ImportAsync(
            Stream csvStream, string fileName, int adminUserId, CancellationToken ct = default)
        {
            var (records, parseErrors) = parser.Parse(csvStream);
            var errors = new List<ImportError>(parseErrors);

            if (records.Count == 0 && errors.Count == 0)
                throw new DomainException("The uploaded file contains no data rows.");

            var batch = new ImportBatch
            {
                FileName = fileName,
                ImportedByUserId = adminUserId,
                TotalRows = records.Count + parseErrors.Count
            };
            db.ImportBatches.Add(batch);

            await db.SaveChangesAsync(ct);

            var ids = records.Select(r => r.CustomerExternalId).Distinct().ToList();
            var rewards = (await db.CustomerRewards
                .AsQueryable()
                .Where(r => ids.Contains(r.CustomerExternalId))
                .ToListAsync(ct))
                .ToDictionary(r => r.CustomerExternalId);

            int matched = 0, unmatched = 0, alreadyProcessed = 0;

            foreach (var record in records)
            {
                if (!rewards.TryGetValue(record.CustomerExternalId, out var reward))
                {
                    unmatched++;
                    continue;
                }

                if (reward.Status == RewardStatus.Cancelled)
                {
                    unmatched++;
                    errors.Add(new ImportError(0,
                        $"Customer {record.CustomerExternalId} has a cancelled reward and was skipped."));
                    continue;
                }

                if (reward.DiscountOfferAccepted)
                {
                    alreadyProcessed++;
                    continue;
                }

                if (record.PurchaseDate < reward.RewardDate)
                {
                    unmatched++;
                    errors.Add(new ImportError(0,
                        $"Customer {record.CustomerExternalId}: purchase date {record.PurchaseDate:yyyy-MM-dd} " +
                        $"is before the reward date {reward.RewardDate:yyyy-MM-dd}."));
                    continue;
                }

                reward.MarkPurchased(record.PurchaseDate, batch.Id);
                matched++;
            }

            batch.MatchedRows = matched;
            batch.UnmatchedRows = unmatched;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Import {BatchId} from {FileName}: {Matched} matched, {Unmatched} unmatched, {Already} already processed",
                batch.Id, fileName, matched, unmatched, alreadyProcessed);

            return new ImportResultDto(batch.Id, fileName, batch.TotalRows, matched, unmatched, alreadyProcessed, errors);
        }
    }
}
