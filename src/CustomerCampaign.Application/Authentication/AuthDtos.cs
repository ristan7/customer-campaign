using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CustomerCampaign.Application.Authentication
{
    public record LoginRequest(
    [Required, MaxLength(50)] string Username,
    [Required, MaxLength(100)] string Password);

    public record ClientTokenRequest(
        [Required, MaxLength(100)] string ClientId,
        [Required, MaxLength(200)] string ClientSecret);

    public record TokenResponse(
        string AccessToken,
        string TokenType,
        int ExpiresIn,
        string Subject,
        string Role);
}
