using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Campaigns
{
    public interface ICampaignService
    {
        Task<CampaignResultsDto> GetResultsAsync(int campaignId, CancellationToken ct = default);
        Task<CampaignResultsDto> GetCurrentResultsAsync(CancellationToken ct = default);
    }
}
