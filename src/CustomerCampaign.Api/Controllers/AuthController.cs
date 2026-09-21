using CustomerCampaign.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [AllowAnonymous]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
            => Ok(await authService.LoginAsync(request, ct));

        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> Token(ClientTokenRequest request, CancellationToken ct)
            => Ok(await authService.IssueClientTokenAsync(request, ct));
    }
}
