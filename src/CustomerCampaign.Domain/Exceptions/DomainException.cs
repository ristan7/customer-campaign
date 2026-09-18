using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}
