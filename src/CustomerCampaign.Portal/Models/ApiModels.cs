using System.ComponentModel.DataAnnotations;

namespace CustomerCampaign.Portal.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public record TokenResponse(string AccessToken, string TokenType, int ExpiresIn, string Subject, string Role);

    public record CustomerInfo(int ExternalId, string FullName, DateOnly? DateOfBirth, int? Age, string? City, string? State);

    public record RewardDto(
        int Id, int CampaignId, int AgentId, string AgentName,
        int CustomerExternalId, string CustomerName, string? CustomerCity,
        DateOnly RewardDate, string Status, bool DiscountOfferAccepted,
        DateOnly? PurchaseDate, string? Note, DateTime CreatedAtUtc);

    public record AgentDailyRewardsDto(
        DateOnly Date, int DailyLimit, int Used, int Remaining, IReadOnlyList<RewardDto> Items);

    public record AgentPerformanceDto(int AgentId, string AgentName, int Rewarded, int Accepted, decimal ConversionRate);

    public record DailyBreakdownDto(DateOnly Date, int Rewarded, int Accepted);

    public record CampaignResultsDto(
        int CampaignId, string Name, DateOnly StartDate, DateOnly EndDate,
        int DailyLimitPerAgent, decimal DiscountPercent,
        int TotalRewarded, int TotalAccepted, decimal ConversionRate, int CancelledCount,
        IReadOnlyList<AgentPerformanceDto> AgentPerformance,
        IReadOnlyList<DailyBreakdownDto> DailyBreakdown);

    public record ProblemDetailsResponse(string? Title, int? Status, string? Detail);

    public class DashboardViewModel
    {
        public AgentDailyRewardsDto? Daily { get; set; }
        public int? CustomerExternalId { get; set; }
        public string? Note { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public record ImportError(int RowNumber, string Message);

    public record ImportResultDto(
        int ImportBatchId, string FileName, int TotalRows,
        int MatchedRows, int UnmatchedRows, int AlreadyProcessedRows,
        IReadOnlyList<ImportError> Errors);
}
