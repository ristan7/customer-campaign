using CustomerCampaign.Application.Common.Models;
using CustomerCampaign.Application.Rewards;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers
{
    [ApiController]
    [Route("api/v1/rewards")]
    public class RewardsController(IRewardService rewardService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<RewardDto>> Create(
            [FromHeader(Name = "X-Agent-Id")] int agentId,
            CreateRewardRequest request,
            CancellationToken ct)
        {
            var reward = await rewardService.CreateAsync(agentId, request, ct);
            return StatusCode(StatusCodes.Status201Created, reward);
        }

        [HttpGet("my")]
        public async Task<ActionResult<AgentDailyRewardsDto>> GetMy(
            [FromHeader(Name = "X-Agent-Id")] int agentId,
            [FromQuery] DateOnly? date,
            CancellationToken ct)
            => Ok(await rewardService.GetMyRewardsAsync(agentId, date, ct));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Cancel(
            [FromHeader(Name = "X-Agent-Id")] int agentId,
            int id,
            CancellationToken ct)
        {
            await rewardService.CancelAsync(agentId, id, ct);
            return NoContent();
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<RewardDto>>> GetAll([FromQuery] RewardQuery query, CancellationToken ct)
            => Ok(await rewardService.GetRewardsAsync(query, ct));
    }
}
