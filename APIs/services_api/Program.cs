using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Winton.Extensions.Configuration.Consul;

namespace services_api
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((context, cfgBuilder) =>
                {
                    cfgBuilder.AddEnvironmentVariables();
                    var environment = context.HostingEnvironment.EnvironmentName;

                    string keys = Environment.GetEnvironmentVariable("CONFIGURATION_KEYS");
                    if (string.IsNullOrWhiteSpace(keys)) return;

                    foreach (var key in keys.Split(",").Select(x => $"{x.Trim()}/appsettings.{environment}.json"))
                    {
                        cfgBuilder.AddConsul(key, options =>
                        {
                            options.ConsulConfigurationOptions = o => { o.Address = new Uri(Environment.GetEnvironmentVariable("CONSUL_URL")); };
                            options.Optional = true;

                            //Wait Time before pulling a change from Consul
                            //options.PollWaitTime = TimeSpan.FromSeconds(5);
                            options.ReloadOnChange = true;
                           // options.KeyToRemove = key;


                            options.OnLoadException = (consulLoadExceptionContext) =>
                            {
                                Log.Information($"Error onLoadException {consulLoadExceptionContext.Exception.Message}");
                                Log.Error($"Error onLoadException {consulLoadExceptionContext.Exception} and stacktrace {consulLoadExceptionContext.Exception.StackTrace}");
                                throw consulLoadExceptionContext.Exception;
                            };

                            options.OnWatchException = (consulWatchExceptionContext) =>
                            {
                                Log.Information($"Unable to watchChanges in Consul due to {consulWatchExceptionContext.Exception.Message}. Check error logs for details");
                                Log.Error($"Unable to watchChanges in Consul due to {consulWatchExceptionContext.Exception.StackTrace}");

                                return TimeSpan.FromSeconds(2);
                            };
                        });
                    }


                });
    }
}
