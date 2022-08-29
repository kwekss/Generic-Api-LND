using AuthService.Features;
using helpers;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService
{
    public static class AuthServiceExtension
    {
        public static IServiceCollection AddAuthService(this IServiceCollection services)
        {
            services
                .AddSingleton<BaseServiceFeature, VerifyUserCredentialsFeature>();

            return services;
        }
    }
}
