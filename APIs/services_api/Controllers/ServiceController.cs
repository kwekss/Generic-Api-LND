using helper.Logger;
using helpers;
using helpers.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace services_api.Controllers
{
    //[Route("[controller]")]
    public class ServiceController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileLogger _logger;
        private readonly bool _is_logging_enabled;

        public ServiceController(IServiceProvider serviceProvider, IFileLogger fileLogger, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = fileLogger;
            _is_logging_enabled = config.GetValue("ENABLE_LOGGING", false);
        }

        [HttpPost, Route("{serviceName}/{featureName}")]
        public async Task<ApiResponse> ServiceExecutor(string serviceName, string featureName)
        {
            StringBuilder logs = new StringBuilder();
            Guid requestId = Guid.NewGuid();
            logs.AppendLine($"[{requestId}] Request start @ {DateTime.Now}");

            Event.Subscribe += (string type, dynamic[] data) =>
            {
                if (type.ToLower() == "log")
                    for (int i = 0; i < data.Length; i++)
                    {
                        logs.AppendLine($"[{requestId}] {data[i]}");
                    } 
            };
            
            try
            {
                var feature = _serviceProvider.GetServices<BaseServiceFeature>()
                .FirstOrDefault(_ => _.Service.Trim().ToLower() == serviceName.Trim().ToLower() &&
                    _.FeatureName.Trim().ToLower() == featureName.Trim().ToLower());

                if (feature == null) return new ApiResponse { ResponseMessage = "Service or feature not found" };

                var featureResponse = await feature.ExecuteFeature();

                logs.AppendLine($"[{requestId}] Response: {featureResponse.Stringify()}");
                if (_is_logging_enabled) _logger.LogInfo(logs);

                return featureResponse;
            }
            catch (Exception e)
            {
                _logger.LogInfo(logs);
                _logger.LogError(e);
                return new ApiResponse { Success = false, ResponseMessage = "A system error occured. Please try again" };
            }
        }

    }
}
