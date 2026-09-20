using CustomerCampaign.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure.Time
{
    public class CampaignClock : IClock
    {
        private readonly TimeZoneInfo _timeZone;

        public CampaignClock(IConfiguration configuration, ILogger<CampaignClock> logger)
        {
            var id = configuration["Campaign:TimeZone"] ?? "Europe/Belgrade";
            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                logger.LogWarning("Time zone {TimeZone} not found, falling back to UTC.", id);
                _timeZone = TimeZoneInfo.Utc;
            }
        }

        public DateTime UtcNow => DateTime.UtcNow;

        public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _timeZone));
    }
}
