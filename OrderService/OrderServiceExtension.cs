using helpers;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Features;
using OrderService.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService
{
    public static class OrderServiceExtension
    {
        public static IServiceCollection AddOrderService(this IServiceCollection services)
        {
            services.AddSingleton<IDatabaseProvider, DatabaseProvider>();

            services.AddSingleton<BaseServiceFeature, GetAllOrdersFeature>();


            return services;
        }
    }
}
