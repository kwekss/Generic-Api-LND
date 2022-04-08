using helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using VendorService.Features;
using VendorService.Providers;

namespace VendorService
{
    public static class VendorServiceExtension
    {
        public static IServiceCollection AddVendorService(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, DatabaseProvider>();

            services.AddSingleton<BaseServiceFeature, GetAllVendorsFeature>();
            services.AddSingleton<BaseServiceFeature, AddVendorFeature>();


            return services;
        }
    }
}
