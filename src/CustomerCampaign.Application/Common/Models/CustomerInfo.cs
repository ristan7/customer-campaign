using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Models
{
    public record CustomerInfo(
        int ExternalId,
        string FullName,
        DateOnly? DateOfBirth,
        int? Age,
        string? City,
        string? State);




}
