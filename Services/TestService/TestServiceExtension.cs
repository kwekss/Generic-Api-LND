using helpers;
using Microsoft.Extensions.DependencyInjection;
using TestService.Features;

namespace BundleService
{
    public static class TestServiceExtension
    {
        public static IServiceCollection AddTestService(this IServiceCollection services)
        {
            services
                .AddSingleton<BaseServiceFeature, TestFeature>();
             
            return services;
        }
    }

}
