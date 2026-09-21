using CustomerCampaign.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Rewards
{
    public interface IRewardService
    {
        Task<RewardDto> CreateAsync(int agentId, CreateRewardRequest request, CancellationToken ct = default);
        Task<AgentDailyRewardsDto> GetMyRewardsAsync(int agentId, DateOnly? date, CancellationToken ct = default);
        Task CancelAsync(int agentId, int rewardId, CancellationToken ct = default);
        Task<PagedResult<RewardDto>> GetRewardsAsync(RewardQuery query, CancellationToken ct = default);
    }
}
