using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Authentication
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<TokenResponse> IssueClientTokenAsync(ClientTokenRequest request, CancellationToken ct = default);
    }
}
