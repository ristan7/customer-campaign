using CustomerCampaign.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Security
{
    public class PasswordHasherService : IPasswordHasher
    {
        private static readonly object Owner = new();
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password) => _hasher.HashPassword(Owner, password);

        public bool Verify(string hash, string password) =>
            _hasher.VerifyHashedPassword(Owner, hash, password) != PasswordVerificationResult.Failed;
    }
}
