using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Campaigns
{
    public record CampaignResultsDto(
    int CampaignId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    int DailyLimitPerAgent,
    decimal DiscountPercent,
    int TotalRewarded,
    int TotalAccepted,
    decimal ConversionRate,
    int CancelledCount,
    IReadOnlyList<AgentPerformanceDto> AgentPerformance,
    IReadOnlyList<DailyBreakdownDto> DailyBreakdown);

    public record AgentPerformanceDto(
        int AgentId, string AgentName, int Rewarded, int Accepted, decimal ConversionRate);

    public record DailyBreakdownDto(DateOnly Date, int Rewarded, int Accepted);
}
