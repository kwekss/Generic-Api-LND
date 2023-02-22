using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Text.RegularExpressions;

namespace helpers.Engine
{
    public class LogEnricher : ILogEventEnricher
    {
        private readonly int _truncate_at;

        public LogEnricher(IConfiguration config)
        {
            _truncate_at = config.GetValue("Utility:Logging:TRUNCATE_AT", 0);
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string message = logEvent.RenderMessage();
            if (logEvent.Level == LogEventLevel.Information)
            {
                var actual_length = message.Length;

                Regex regex_newline = new Regex("(\r\n|\r|\n)");
                if (!string.IsNullOrWhiteSpace(message) && message.Length > 0)
                {
                    message = regex_newline.Replace(message, "");
                    if (_truncate_at > 0)
                    {
                        var will_truncate = message.Length > _truncate_at;
                        message = message.Substring(0, Math.Min(message.Length, message.Length > _truncate_at ? _truncate_at : message.Length));
                        if (will_truncate) message = $"{message}...[TRUNCATED |LENGHT:{actual_length}]";
                    }
                }

            };


            var logEventProperty = propertyFactory.CreateProperty("AppLog", message);

            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }
}
