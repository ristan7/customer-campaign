using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.ExternalServices.FindPerson
{
    public class FindPersonOptions
    {
        public const string SectionName = "FindPerson";
        public string Endpoint { get; set; } = string.Empty;
        public string Namespace { get; set; } = "http://tempuri.org";
        public string SoapAction { get; set; } = string.Empty;
    }
}
