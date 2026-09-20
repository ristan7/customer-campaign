using CustomerCampaign.Application.Campaigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers
{
    [ApiController]
    [Route("api/v1/campaigns")]
    [Authorize(Policy = "ReadCampaignData")]
    public class CampaignsController(ICampaignService campaignService) : ControllerBase
    {
        [HttpGet("current/results")]
        public async Task<ActionResult<CampaignResultsDto>> GetCurrent(CancellationToken ct)
            => Ok(await campaignService.GetCurrentResultsAsync(ct));

        [HttpGet("{id:int}/results")]
        public async Task<ActionResult<CampaignResultsDto>> GetById(int id, CancellationToken ct)
            => Ok(await campaignService.GetResultsAsync(id, ct));
    }
}
