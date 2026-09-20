using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Models
{
    public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
}
