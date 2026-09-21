using CustomerCampaign.Application.Authentication;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomerCampaign.Infrastructure.Security
{
    public class JwtTokenGenerator(IOptions<JwtOptions> options) : ITokenGenerator
    {
        private readonly JwtOptions _options = options.Value;

        public TokenResponse CreateForUser(int userId, string username, string role) =>
            Create(userId.ToString(), username, role);

        public TokenResponse CreateForClient(string clientId, string name) =>
            Create(clientId, name, Roles.Integration);

        private TokenResponse Create(string subject, string name, string role)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Name, name),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
            var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new TokenResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                "Bearer",
                _options.AccessTokenMinutes * 60,
                subject,
                role);
        }
    }
}
