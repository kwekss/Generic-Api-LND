using BackgroundService;
using helpers.Engine;
using helpers.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using models;
using System.Collections.Generic;
using System.Linq;
using TestService;

namespace services_api
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddAppDependencies(_config)
                .AddTestService()
                .AddBackgroudService()
                ;

            services
                .AddCors()
                .AddSingleton(x => services)
                .AddSingleton<BackgroundRunner>()
                .AddControllers();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Start Background Service And all AutoRun Automatically
            app.ApplicationServices.GetRequiredService<BackgroundRunner>();

            app.UseRouting();
            app.UseCors(o =>
            {
                var allowedOrigins = _config.GetSection("AllowedOrigins").Get<string[]>();
                if (allowedOrigins.Length > 0)
                    for (var i = 0; i < allowedOrigins.Length; i++)
                        o.WithOrigins(allowedOrigins[i]);
                o.AllowAnyHeader();
            });

            app.UseMiddleware<ServiceFeatureMiddleware>(new MiddlewareOption
            {
                CustomErrorResponse = (statusCode, message) => new CustomApiResponse { StatusCode = message }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public  class CustomApiResponse
    {
        public bool Success { get; set; }
        public string StatusCode { get; set; }
    }
}
