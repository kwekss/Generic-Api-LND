using helpers;
using Microsoft.Extensions.DependencyInjection;
using TestService.Features;
using TestService.Providers;

namespace TestService
{
    public static class TestServiceExtension
    {
        public static IServiceCollection AddTestService(this IServiceCollection services)
        {
            services
                .AddSingleton<IUploadProvider, UploadProvider>()
                .AddSingleton<BaseServiceFeature, TestFeature>()
                .AddSingleton<BaseServiceFeature, TestOpenIdToken>()
                .AddSingleton<BaseServiceFeature, UploadDocument>();

            return services;
        }
    }

}
