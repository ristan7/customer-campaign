using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Exceptions
{
    public class NotFoundException(string message) : Exception(message);
}
