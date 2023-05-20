using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace helpers.Engine
{
    public class LogEnricher : ILogEventEnricher
    {
        private readonly int _truncate_at;
        private readonly List<LogMask> _mask_patterns;

        public LogEnricher(IConfiguration config)
        {
            _truncate_at = config.GetValue("Utility:Logging:TRUNCATE_AT", 0);
            _mask_patterns = config.GetSection("Utility:Logging:Masking").Get<List<LogMask>>();
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

            if (_mask_patterns != null && _mask_patterns.Any() && !string.IsNullOrWhiteSpace(message))
            {
                for (int i = 0; i < _mask_patterns.Count; i++)
                { 
                    message = Regex.Replace(message, _mask_patterns[i].Pattern, _mask_patterns[i].Mask, RegexOptions.IgnoreCase);
                }
            }
            //string invokeSpec = "\":{\"Account\":\"d\\\\adm\",\"password\":\"cWExZjEiMTM=\"},\"SqlServer\":{\"InstanceName\":\"\",\"MachineName\":\"MyMachine\",\"Port\":null}";
            //var pattern = "(\\\"password\\\":(\\s*)?)\\\"[^\\\"]*(\\\")";
            //var replaced = Regex.Replace(invokeSpec, pattern, "$1***$2", RegexOptions.IgnoreCase);
            //Console.WriteLine(replaced);

            var logEventProperty = propertyFactory.CreateProperty("AppLog", message);

            logEvent.AddPropertyIfAbsent(logEventProperty);
        }
    }

    internal class LogMask
    {
        public string Pattern { get; set; }
        public string Mask { get; set; }
    }
}
