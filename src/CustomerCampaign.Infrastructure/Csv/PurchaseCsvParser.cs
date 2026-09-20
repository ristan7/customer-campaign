using CsvHelper;
using CsvHelper.Configuration;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Imports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CustomerCampaign.Infrastructure.Csv
{
    public class PurchaseCsvParser : IPurchaseCsvParser
    {
        private static readonly string[] DateFormats =
            ["yyyy-MM-dd", "dd.MM.yyyy", "dd/MM/yyyy", "MM/dd/yyyy"];

        public (IReadOnlyList<PurchaseRecord> Records, IReadOnlyList<ImportError> Errors) Parse(Stream csvStream)
        {
            var records = new List<PurchaseRecord>();
            var errors = new List<ImportError>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant(),
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, config);

            if (!csv.Read() || !csv.ReadHeader())
                return (records, [new ImportError(1, "The CSV file has no header row.")]);

            var row = 1;
            while (csv.Read())
            {
                row++;

                var rawId = csv.TryGetField<string>("customerid", out var idValue) ? idValue : null;
                var rawDate = csv.TryGetField<string>("purchasedate", out var dateValue) ? dateValue : null;
                csv.TryGetField<string>("orderreference", out var orderRef);

                if (!int.TryParse(rawId, out var customerId) || customerId <= 0)
                {
                    errors.Add(new ImportError(row, $"Invalid or missing CustomerId: '{rawId}'."));
                    continue;
                }

                if (!TryParseDate(rawDate, out var purchaseDate))
                {
                    errors.Add(new ImportError(row, $"Invalid or missing PurchaseDate: '{rawDate}'."));
                    continue;
                }

                records.Add(new PurchaseRecord(customerId, purchaseDate, string.IsNullOrWhiteSpace(orderRef) ? null : orderRef));
            }

            return (records, errors);
        }

        private static bool TryParseDate(string? value, out DateOnly date)
        {
            date = default;
            return !string.IsNullOrWhiteSpace(value)
                   && DateOnly.TryParseExact(value.Trim(), DateFormats,
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}
