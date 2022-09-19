using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace helpers.Engine
{
    public static class BackgroundServiceExtension
    {
        public static IServiceCollection AddBackgroundDependencies(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddSharedService(config)
                ;

            return services;
        }
    }
}
