using BackgroundService;
using helpers.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace queue_background_services
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly IServiceProvider provider;
        public IServiceProvider Provider => provider;
        public Startup()
        {
            var environment = "Production";

#if DEBUG
            environment = "Development";
#endif

            configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{environment}.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();

            var services = new ServiceCollection();


            services
                .AddSingleton(configuration)
                .AddBackgroundDependencies(configuration)
                .AddBackgroudService()
                ;

            services
                .AddSingleton<BackgroundRunner>();

            // build the pipeline
            provider = services.BuildServiceProvider();
        }

        public T GetService<T>() => Provider.GetRequiredService<T>();
        public IEnumerable<T> GetServices<T>() => Provider.GetServices<T>();
    }
}
