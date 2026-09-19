using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Exceptions
{
    public class ExternalServiceException : Exception
    {
        public ExternalServiceException(string message, Exception? inner = null)
        : base(message, inner) { }
    }
}
