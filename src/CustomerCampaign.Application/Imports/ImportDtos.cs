using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Imports
{
    public record PurchaseRecord(int CustomerExternalId, DateOnly PurchaseDate, string? OrderReference);

    public record ImportError(int RowNumber, string Message);

    public record ImportResultDto(
        int ImportBatchId,
        string FileName,
        int TotalRows,
        int MatchedRows,
        int UnmatchedRows,
        int AlreadyProcessedRows,
        IReadOnlyList<ImportError> Errors);
}
