using helper.Logger;
using helpers.Database;
using helpers.Database.Executors;
using helpers.Database.Models;
using helpers.Engine;
using helpers.Middlewares;
using helpers.Notifications;
using helpers.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
            DatabaseConnections databaseConnections = new DatabaseConnections();
            JToken db2 = JObject.FromObject(new { });
            _config.Bind("Databases", databaseConnections);
            //_config.Bind("Databases", db2);

            var db = _config.GetValue<JToken>("Databases");

            services
                .AddSingleton(databaseConnections)
                .AddHttpClient()
                .AddHttpContextAccessor() 
                .AddTransient<IFileLogger, FileLogger>(x => new FileLogger(_config.GetValue("LOG_DIR", "")))
                .AddTransient<IFeatureContext, FeatureContext>()
                .AddSingleton<IStoredProcedureExecutor, NpgsqlStoredProcedureExecutor>()
                .AddSingleton<IDBHelper, DBHelper>()
                .AddTransient<IHttpHelper, HttpHelper>()
                .AddSingleton<ISmsNotification, SmsNotification>()
                .AddSingleton<IMongoDBHelper, MongoDBHelper>()
                .AddSingleton<ISessionManager, SessionManager>()
                .AddTestService()
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
            // Start Backgroun Service Automatically
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

            app.UseMiddleware<ServiceFeatureMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
