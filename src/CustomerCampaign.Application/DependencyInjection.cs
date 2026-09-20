using CustomerCampaign.Application.Authentication;
using CustomerCampaign.Application.Campaigns;
using CustomerCampaign.Application.Imports;
using CustomerCampaign.Application.Rewards;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IRewardService, RewardService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IPurchaseImportService, PurchaseImportService>();
            services.AddScoped<ICampaignService, CampaignService>();

            return services;
        }
    }
}
