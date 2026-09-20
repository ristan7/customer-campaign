using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Infrastructure.ExternalServices.FindPerson;
using CustomerCampaign.Infrastructure.Persistence;
using CustomerCampaign.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using CustomerCampaign.Infrastructure.Time;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration.GetValue("DatabaseProvider", DatabaseProvider.MySql);

            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'Default' is not configured.");

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (provider)
                {
                    case DatabaseProvider.MySql:
                        var version = configuration["MySqlServerVersion"] ?? "8.0.42";
                        options.UseMySql(connectionString, ServerVersion.Parse(version));
                        break;
                    case DatabaseProvider.SqlServer:
                        options.UseSqlServer(connectionString);
                        break;
                    case DatabaseProvider.PostgreSql:
                        options.UseNpgsql(connectionString);
                        break;
                    default:
                        throw new NotSupportedException($"Database provider '{provider}' is not supported.");
                }
            });

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
            services.AddSingleton<IPasswordHasher, PasswordHasherService>();
            services.AddScoped<DbInitializer>();

            services.Configure<FindPersonOptions>(configuration.GetSection(FindPersonOptions.SectionName));

            var directoryProvider = configuration["CustomerDirectory:Provider"] ?? "Soap";

            if (string.Equals(directoryProvider, "Stub", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<ICustomerDirectory, StubCustomerDirectory>();
            }
            else
            {
                services.AddHttpClient<ICustomerDirectory, FindPersonSoapClient>()
                    .AddStandardResilienceHandler(o =>
                    {
                        o.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);
                        o.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(12);
                        o.Retry.MaxRetryAttempts = 1;
                        o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                    });
            }

            services.AddSingleton<IClock, CampaignClock>();

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
