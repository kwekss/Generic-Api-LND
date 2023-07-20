using helpers.Database;
using helpers.Database.Executors;
using helpers.Database.Models;
using helpers.Interfaces;
using helpers.Notifications;
using helpers.Session;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace helpers.Engine
{
    public static class ApiServiceExtension
    {
        public static IServiceCollection AddSharedService(this IServiceCollection services, IConfiguration config)
        {

            List<Connection> databaseConnections = new List<Connection>();
            config.Bind("Databases", databaseConnections);

            Utility.COUNTRY_CODE = config.GetValue("Utility:CountryCode", "233");
            var enableConsoleLog = config.GetValue("Utility:Logging:EnableConsoleLogs", true);

            var logLevel = config.GetValue("Utility:Logging:Level", "Information");

            var enableFileLog = config.GetValue("Utility:Logging:EnableFileLogs", true);
            bool useBuiltInIntegratorStorage = config.GetValue("Utility:Authentication:Integrator:UseBuiltInStorage", true);

            string logPath = config.GetValue("LOG_DIR", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            var logger = new LoggerConfiguration()
                            .MinimumLevel.Information()
                            ;

            if (logLevel == "Information") logger = logger.MinimumLevel.Information();
            if (logLevel == "Verbose") logger = logger.MinimumLevel.Verbose();
            if (logLevel == "Debug") logger = logger.MinimumLevel.Debug();
            if (logLevel == "Warning") logger = logger.MinimumLevel.Warning();
            if (logLevel == "Error") logger = logger.MinimumLevel.Error();

            if (enableConsoleLog) logger = logger.WriteTo.Console();

            if (enableFileLog)
            {
                logger = logger.WriteTo.Map(evt => evt.Level, (level, wt) => wt.File(
                                 Path.Combine(logPath, $"{level}", $"{DateTime.UtcNow:yyyy-MM-dd}.log"),
                                 rollingInterval: RollingInterval.Day,
                                 outputTemplate: "{Timestamp:yyyy-MM-dd hh:mm:ss tt} [{CorrelationId}] - {AppLog}{NewLine}{Exception}"
                                 )
                             );
            }

            logger = logger.Enrich.With(new LogEnricher(config))
                           .Enrich.FromLogContext();

            Log.Logger = logger.CreateLogger();

            var messageHub = new MessengerHub();

            if (useBuiltInIntegratorStorage)
                services.AddSingleton<IIntegratorStorage, IntegratorStorage>();

            services
                .AddSingleton(databaseConnections)
                .AddSingleton<IMessengerHub>(messageHub)
                .AddHttpClient()
                .AddTransient<IFeatureContext, FeatureContext>()
                .AddSingleton<IStoredProcedureExecutor, NpgsqlStoredProcedureExecutor>()
                .AddSingleton<IOracleExecutor, OracleExecutor>()
                .AddSingleton<IDBHelper, DBHelper>()
                .AddSingleton<IOracleDBHelper, OracleDBHelper>()
                .AddSingleton<IHttpHelper, HttpHelper>()
                .AddSingleton<ISmsNotification, SmsNotification>()
                .AddSingleton<IIntegratorHelper, IntegratorHelper>()
                .AddSingleton<IMongoDBHelper, MongoDBHelper>()
                .AddSingleton<IDocumentationBuilder, DocumentationBuilder>()
                ;

            return services;
        }
        public static IServiceCollection AddAppDependencies(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddSharedService(config)
                .AddSingleton<ISessionManager, SessionManager>()
                ;

            return services;
        }
    }
}
