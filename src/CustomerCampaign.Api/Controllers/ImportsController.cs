using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Imports;
using CustomerCampaign.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers
{
    [ApiController]
    [Route("api/v1/imports")]
    [Authorize(Policy = "AdminOnly")]
    public class ImportsController(IPurchaseImportService importService, ICurrentUser currentUser) : ControllerBase
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        [HttpPost("purchases")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<ActionResult<ImportResultDto>> ImportPurchases(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                throw new DomainException("No file was uploaded.");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                throw new DomainException("Only .csv files are supported.");

            var adminId = currentUser.UserId
                ?? throw new UnauthorizedException("Token does not contain a valid user identifier.");

            await using var stream = file.OpenReadStream();
            return Ok(await importService.ImportAsync(stream, file.FileName, adminId, ct));
        }
    }
}
