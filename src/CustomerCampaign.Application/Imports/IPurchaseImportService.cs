using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Imports
{
    public interface IPurchaseImportService
    {
        Task<ImportResultDto> ImportAsync(Stream csvStream, string fileName, int adminUserId, CancellationToken ct = default);
    }
}
