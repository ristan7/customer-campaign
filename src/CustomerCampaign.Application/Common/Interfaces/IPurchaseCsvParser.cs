using CustomerCampaign.Application.Imports;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application.Common.Interfaces
{
    public interface IPurchaseCsvParser
    {
        (IReadOnlyList<PurchaseRecord> Records, IReadOnlyList<ImportError> Errors) Parse(Stream csvStream);
    }
}
