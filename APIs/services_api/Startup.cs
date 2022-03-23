using helper.Logger;
using helpers.Database;
using helpers.Database.Executors;
using helpers.Database.Models;
using helpers.Engine;
using helpers.Middlewares;
using helpers.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            _config.Bind("Databases", databaseConnections);

            services
                .AddSingleton(databaseConnections)
                .AddHttpClient()
                .AddHttpContextAccessor()
                .AddTransient<IFileLogger, FileLogger>(x => new FileLogger(_config.GetValue("LOG_DIR", "")))
                .AddSingleton<IFeatureContext, FeatureContext>()
                .AddSingleton<IStoredProcedureExecutor, NpgsqlStoredProcedureExecutor>()
                .AddSingleton<ISmsNotification, SmsNotification>()
                .AddSingleton<IDBHelper, DBHelper>()
                .AddTestService();
             
            services
                .AddSingleton(x => services) .AddControllers();
        }

       
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseMiddleware<ServiceFeatureMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
