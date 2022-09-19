using BackgroundService.AutoRun;
using BackgroundService.Jobs;
using helpers.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService
{
    public static class BackgroundServiceExtension
    {
        public static IServiceCollection AddBackgroudService(this IServiceCollection services)
        {
            services
                .AddSingleton<IAutoRun, TestAutoRun>()
                .AddSingleton<IBackgroundJob, TestBackgroundWorker>()
                ;

            return services;
        }
    }
}
