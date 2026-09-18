using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hash, string password);
    }
}
