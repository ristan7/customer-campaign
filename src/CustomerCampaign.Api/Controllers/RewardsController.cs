using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Common.Models;
using CustomerCampaign.Application.Rewards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/rewards")]
[Authorize]
public class RewardsController(IRewardService rewardService, ICurrentUser currentUser) : ControllerBase
{
    private int AgentId => currentUser.UserId
        ?? throw new UnauthorizedException("Token does not contain a valid user identifier.");

    [HttpPost]
    [Authorize(Policy = "AgentOnly")]
    public async Task<ActionResult<RewardDto>> Create(CreateRewardRequest request, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await rewardService.CreateAsync(AgentId, request, ct));

    [HttpGet("my")]
    [Authorize(Policy = "AgentOnly")]
    public async Task<ActionResult<AgentDailyRewardsDto>> GetMy([FromQuery] DateOnly? date, CancellationToken ct)
        => Ok(await rewardService.GetMyRewardsAsync(AgentId, date, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AgentOnly")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        await rewardService.CancelAsync(AgentId, id, ct);
        return NoContent();
    }

    [HttpGet]
    [Authorize(Policy = "ReadCampaignData")]
    public async Task<ActionResult<PagedResult<RewardDto>>> GetAll([FromQuery] RewardQuery query, CancellationToken ct)
        => Ok(await rewardService.GetRewardsAsync(query, ct));
}