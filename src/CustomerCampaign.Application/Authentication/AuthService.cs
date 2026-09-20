using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Authentication
{
    public class AuthService(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator,
    ILogger<AuthService> logger) : IAuthService
    {
        public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, ct);

            if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
            {
                logger.LogWarning("Failed login attempt for username {Username}", request.Username);
                throw new UnauthorizedException("Invalid username or password.");
            }

            logger.LogInformation("User {UserId} logged in", user.Id);
            return tokenGenerator.CreateForUser(user.Id, user.Username, user.Role.ToString());
        }

        public async Task<TokenResponse> IssueClientTokenAsync(ClientTokenRequest request, CancellationToken ct = default)
        {
            var client = await db.ApiClients.FirstOrDefaultAsync(c => c.ClientId == request.ClientId && c.IsActive, ct);

            if (client is null || !passwordHasher.Verify(client.ClientSecretHash, request.ClientSecret))
            {
                logger.LogWarning("Failed client authentication for {ClientId}", request.ClientId);
                throw new UnauthorizedException("Invalid client credentials.");
            }

            logger.LogInformation("API client {ClientId} authenticated", client.ClientId);
            return tokenGenerator.CreateForClient(client.ClientId, client.Name);
        }
    }
}
