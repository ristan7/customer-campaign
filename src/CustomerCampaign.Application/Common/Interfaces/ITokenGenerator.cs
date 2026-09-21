using CustomerCampaign.Application.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface ITokenGenerator
    {
        TokenResponse CreateForUser(int userId, string username, string role);
        TokenResponse CreateForClient(string clientId, string name);
    }
}
