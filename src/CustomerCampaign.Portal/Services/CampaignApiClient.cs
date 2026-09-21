using CustomerCampaign.Portal.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CustomerCampaign.Portal.Services
{
    public class CampaignApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<CampaignApiClient> logger)
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public async Task<TokenResponse> LoginAsync(string username, string password, CancellationToken ct = default)
        {
            var response = await http.PostAsJsonAsync("api/v1/auth/login", new { username, password }, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<TokenResponse>(Json, ct))!;
        }

        public async Task<AgentDailyRewardsDto> GetMyRewardsAsync(CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Get, "api/v1/rewards/my");
            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<AgentDailyRewardsDto>(Json, ct))!;
        }

        public async Task<RewardDto> CreateRewardAsync(int customerExternalId, string? note, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Post, "api/v1/rewards");
            request.Content = JsonContent.Create(new { customerExternalId, note });
            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<RewardDto>(Json, ct))!;
        }

        public async Task CancelRewardAsync(int rewardId, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Delete, $"api/v1/rewards/{rewardId}");
            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
        }

        public async Task<CustomerInfo> LookupCustomerAsync(int externalId, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Get, $"api/v1/customers/{externalId}");
            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<CustomerInfo>(Json, ct))!;
        }

        public async Task<CampaignResultsDto> GetCurrentResultsAsync(CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Get, "api/v1/campaigns/current/results");
            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<CampaignResultsDto>(Json, ct))!;
        }

        private HttpRequestMessage Authorized(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            var token = accessor.HttpContext?.User.FindFirst("access_token")?.Value;

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
                return;

            string message;
            try
            {
                var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>(Json, ct);
                message = problem?.Detail ?? problem?.Title ?? response.ReasonPhrase ?? "Request failed.";
            }
            catch
            {
                message = response.ReasonPhrase ?? "Request failed.";
            }

            if (response.StatusCode != HttpStatusCode.UnprocessableEntity)
                logger.LogWarning("API call failed: {Status} {Message}", (int)response.StatusCode, message);

            throw new ApiException((int)response.StatusCode, message);
        }

        public async Task<ImportResultDto> ImportPurchasesAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            using var request = Authorized(HttpMethod.Post, "api/v1/imports/purchases");

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

            var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", fileName);
            request.Content = form;

            var response = await http.SendAsync(request, ct);
            await EnsureSuccessAsync(response, ct);
            return (await response.Content.ReadFromJsonAsync<ImportResultDto>(Json, ct))!;
        }
    }
}
