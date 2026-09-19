using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CustomerCampaign.Infrastructure.ExternalServices.FindPerson
{
    public class FindPersonSoapClient(
        HttpClient httpClient,
        IOptions<FindPersonOptions> options,
        ILogger<FindPersonSoapClient> logger) : ICustomerDirectory
    {
        private readonly FindPersonOptions _options = options.Value;

        public async Task<CustomerInfo?> FindByIdAsync(int externalId, CancellationToken ct = default)
        {
            if (externalId <= 0)
                return null;

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
            {
                Content = new StringContent(BuildEnvelope(externalId), Encoding.UTF8, "text/xml")
            };
            request.Headers.TryAddWithoutValidation("SOAPAction", $"\"{_options.SoapAction}\"");

            string body;
            try
            {
                using var response = await httpClient.SendAsync(request, ct);
                body = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode && !body.Contains("Fault", StringComparison.OrdinalIgnoreCase))
                    throw new ExternalServiceException($"CRM returned HTTP {(int)response.StatusCode}.");
            }
            catch (Exception ex) when (ex is not ExternalServiceException && !ct.IsCancellationRequested)
            {
                logger.LogError(ex, "FindPerson call failed for customer {CustomerId}", externalId);
                throw new ExternalServiceException("CRM service is currently unavailable.", ex);
            }

            return Parse(body, externalId);
        }

        private string BuildEnvelope(int id) => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <FindPerson xmlns="{_options.Namespace}">
              <id>{id}</id>
            </FindPerson>
          </soap:Body>
        </soap:Envelope>
        """;

        private CustomerInfo? Parse(string xml, int externalId)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (XmlException ex)
            {
                throw new ExternalServiceException("CRM returned an invalid response.", ex);
            }

            var fault = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Fault");
            if (fault is not null)
            {
                var message = Child(fault, "faultstring") ?? "Unknown SOAP fault";
                logger.LogWarning("FindPerson SOAP fault for {CustomerId}: {Fault}", externalId, message);
                throw new ExternalServiceException($"CRM error: {message}");
            }

            var result = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "FindPersonResult");
            if (result is null)
                return null;

            var rawName = Child(result, "Name");
            if (string.IsNullOrWhiteSpace(rawName))
                return null;

            var home = result.Elements().FirstOrDefault(e => e.Name.LocalName == "Home");

            DateOnly? dob = DateOnly.TryParseExact(Child(result, "DOB"), "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
            int? age = int.TryParse(Child(result, "Age"), out var a) ? a : null;

            return new CustomerInfo(
                externalId,
                FormatName(rawName),
                dob,
                age,
                home is null ? null : Child(home, "City"),
                home is null ? null : Child(home, "State"));
        }

        private static string? Child(XElement parent, string localName) =>
            parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName)?.Value.Trim();

        private static string FormatName(string raw)
        {
            var parts = raw.Split(',', 2, StringSplitOptions.TrimEntries);
            return parts.Length == 2 ? $"{parts[1]} {parts[0]}" : raw.Trim();
        }

    }
}
