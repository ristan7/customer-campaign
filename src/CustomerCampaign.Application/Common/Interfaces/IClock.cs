using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateOnly Today { get; }
    }
}
