using CustomerCampaign.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace CustomerCampaign.Api.Security
{
    public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
    {
        private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

        public int? UserId =>
            int.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

        public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
    }
}
