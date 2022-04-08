using helpers;
using MenuService.Features;
using MenuService.Providers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MenuService
{
    public static class MenuServiceExtension
    {
        public static IServiceCollection AddMenuService(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, DatabaseProvider>();

            services
                .AddSingleton<BaseServiceFeature, GetMealsByVendorIdFeature>();

            return services;
        }
    }
}
