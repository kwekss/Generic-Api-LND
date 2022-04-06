using helper.Logger;
using helpers.Atttibutes;
using helpers.Exceptions;
using helpers.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace helpers.Middlewares
{
    public class ServiceFeatureMiddleware
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileLogger _logger;
        private readonly IConfiguration _config;
        private readonly RequestDelegate _next;
        private readonly bool _is_logging_enabled;

        public ServiceFeatureMiddleware(IServiceProvider serviceProvider, IFileLogger fileLogger, IConfiguration config, RequestDelegate next)
        {
            _serviceProvider = serviceProvider;
            _logger = fileLogger;
            _config = config;
            _next = next;
            _is_logging_enabled = Convert.ToBoolean(config["ENABLE_LOGGING"] ?? "false");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            StringBuilder logs = new StringBuilder();
            Guid requestId = Guid.NewGuid();
            try
            {

                StartLogger(logs, requestId);

                Event.Dispatch("log", $"Request start @ {DateTime.Now}");

                var path = context.Request.Path.Value.Split('/').Where(_ => !string.IsNullOrWhiteSpace(_)).ToList();
                if (path.Count < 2)
                {
                    await Respond(context, "Service or feature not found");
                    return;
                }

                var endpoint = new Endpoint(path);

                var service = _serviceProvider.GetServices<BaseServiceFeature>()
                    .FirstOrDefault(_ => _.GetType().GetCustomAttributes().Any(_ => _.GetType() == typeof(FeatureAttribute)
                     && (_ as FeatureAttribute).Name.ToLower() == endpoint.Feature.ToLower()) &&
                    _.GetType().Namespace.ToLower() == endpoint.RequiredFullname.ToLower());


                if (service == null)
                {
                    await Respond(context, "Service feature not found");
                    return;
                }


                var route = service.GetType().GetMethods();

                var featureEntry = route.FirstOrDefault(x => x.GetCustomAttributes().Any(_ => _.GetType() == typeof(EntryAttribute) &&
                            (_ as EntryAttribute).Method.ToLower().Split('/')
                                .Select(x => x.Trim()).Contains(context.Request.Method.ToLower())));

                if (featureEntry == null)
                {
                    await Respond(context, "Feature not found");
                    return;
                }

                var entry = (EntryAttribute)featureEntry.GetCustomAttribute(typeof(EntryAttribute));

                RouteRegex regexRoute = null;
                if (!string.IsNullOrWhiteSpace(entry.Route))
                {
                    regexRoute = ConvertRouteToRegex(entry.Route);
                    var regex = Regex.Matches(endpoint.Route, regexRoute.Regex);
                    if (regex.Count == 0)
                    {
                        await Respond(context, "Route not found on feature endpoint");
                        return;
                    }
                    if (regex.Count > 0)
                    {
                        regexRoute.RouteParams.ForEach(_ =>
                        {
                            _.Value = regex[_.Index].Groups[1].Value;
                        });
                    }
                }


                var state = DeterminingAwaitable(featureEntry);
                context.Response.Headers["content-type"] = "application/json";

                InvokeEntryAttributes(service, context, endpoint);

                object featureResponse = await InvokeFeatureEntry(context, service, featureEntry, state, regexRoute);


                logs.AppendLine($"[{requestId}] Response: {featureResponse.Stringify()}");
                if (_is_logging_enabled) _logger.LogInfo(logs);

                await context.Response.WriteAsync(featureResponse.Stringify());
                return;
            }
            catch (ParamerException e)
            {
                _logger.LogInfo(logs);
                _logger.LogError($"[{requestId}]{e}");

                await Respond(context, e.Message);
                return;
            }
            catch (CustomException e)
            {
                _logger.LogInfo(logs);
                _logger.LogError($"[{requestId}]{e}");

                await Respond(context, e.Message);
                return;
            }
            catch (WarningException e)
            {
                _logger.LogInfo(logs);
                _logger.LogWarning($"[{requestId}]{e}");

                await Respond(context, e.Message);
                return;
            }
            catch (Exception e)
            {
                _logger.LogInfo(logs);
                _logger.LogError($"[{requestId}]{e.InnerException ?? e}");

                await Respond(context, "A system error occured. Please try again");
                return;
            }
        }

        private RouteRegex ConvertRouteToRegex(string route)
        {
            RouteRegex routeRegex = null;
            var regex = Regex.Matches(route, "{([a-zA-Z0-9_]*):?(string|int|int64|bool|double)?}");
            if (regex.Count > 0)
            {
                routeRegex = new RouteRegex();
                routeRegex.RouteParams = new List<RouteRegexParam>();
                for (int i = 0; i < regex.Count; i++)
                {
                    var match = regex[i];
                    if (match.Success)
                    {
                        routeRegex.RouteParams.Add(new RouteRegexParam { Index = i, Name = match.Groups[1].Value, Type = match.Groups[2].Value });
                        if (match.Groups[2].Value == "bool")
                            route = route.Replace(match.Value, "(true|false)");

                        if (string.IsNullOrWhiteSpace(match.Groups[2].Value) || match.Groups[2].Value == "string")
                            route = route.Replace(match.Value, "(.*)");
                        if (match.Groups[2].Value == "int" || match.Groups[2].Value == "int64")
                            route = route.Replace(match.Value, "([0-9]*)");
                        if (match.Groups[2].Value == "double")
                            route = route.Replace(match.Value, "(\\d*\\.)?\\d+$");

                    }
                }
                routeRegex.Regex = route;
            }
            return routeRegex;
        }

        private void InvokeEntryAttributes(BaseServiceFeature service, HttpContext context, Endpoint endpoint)
        {

            SetFeatureDefaultValues(context, endpoint, service);
        }

        private async Task<object> InvokeFeatureEntry(HttpContext context, BaseServiceFeature service, MethodInfo matchingFeature, (bool IsAwaitable, bool ReturnData) state, RouteRegex routeRegex)
        {
            object featureResponse = new { };
            if (state.IsAwaitable && state.ReturnData)
                featureResponse = await (dynamic)matchingFeature.Invoke(service, PopulateParameters(matchingFeature.GetParameters(), context, routeRegex));

            if (state.IsAwaitable && !state.ReturnData)
                await (dynamic)matchingFeature.Invoke(service, null);

            if (!state.IsAwaitable && state.ReturnData)
                featureResponse = matchingFeature.Invoke(service, PopulateParameters(matchingFeature.GetParameters(), context, routeRegex));

            if (!state.IsAwaitable && !state.ReturnData)
                matchingFeature.Invoke(service, null);

            return featureResponse;
        }

        private static void SetFeatureDefaultValues(HttpContext context, Endpoint endpoint, BaseServiceFeature service)
        {
            typeof(BaseServiceFeature).GetProperty("Service").SetValue(service, endpoint.Service, null);
            typeof(BaseServiceFeature).GetProperty("FeatureName").SetValue(service, endpoint.Feature, null);
            typeof(BaseServiceFeature).GetProperty("Context").SetValue(service, context, null);
        }

        public void StartLogger(StringBuilder logs, Guid requestId)
        {

            Event.Subscribe += (string type, dynamic[] data) =>
            {
                if (type.ToLower() == "log")
                    for (int i = 0; i < data.Length; i++)
                    {
                        logs.AppendLine($"[{requestId}] {data[i]}");
                    }
            };

        }

        private async Task Respond(HttpContext context, string message)
        {
            context.Response.Headers["content-type"] = "application/json";
            var response = new ApiResponse { Success = false, ResponseMessage = message };
            await context.Response.WriteAsync(response.Stringify());
        }

        private object[] PopulateParameters(IEnumerable<ParameterInfo> parameters, HttpContext httpContext, RouteRegex routeRegex)
        {
            object[] p = new object[parameters.Count()];
            try
            {
                for (int i = 0; i < parameters.Count(); i++)
                {
                    var parameter = parameters.ElementAt(i);
                    var attributes = parameter.GetCustomAttributes(true).ToList();
                    dynamic parameterValue = null;
                    for (int j = 0; j < attributes.Count; j++)
                    {
                        if (attributes[j] is IParameterAttribute)
                        {
                            (attributes[j] as IParameterAttribute).InitAttribute(parameter, httpContext, p);
                        }
                    }
                    if (p[i] == null)
                    {
                        var source = HttpUtility.ParseQueryString(httpContext.Request.QueryString.ToUriComponent());
                        var value = source.Cast<string>().Select(key => new KeyValuePair<string, string>(key.ToLower(), source[key]))
                            .FirstOrDefault(_ => _.Key == parameter.Name.ToLower());
                        if (!string.IsNullOrWhiteSpace(value.Key))
                        {
                            parameterValue = Convert.ChangeType(value.Value, parameter.ParameterType);
                        }

                        if (parameterValue == null && routeRegex != null)
                        {
                            var routeValue = routeRegex.RouteParams.FirstOrDefault(_ => _.Name.ToLower() == parameter.Name.ToLower());
                            if (!string.IsNullOrWhiteSpace(routeValue.Value))
                            {
                                parameterValue = Convert.ChangeType(routeValue.Value, parameter.ParameterType);
                            }
                        }
                        p[i] = parameterValue;
                    }


                }
            }
            catch (Exception e)
            {
                throw new ParamerException(e.Message);
            }

            return p.ToArray();
        }

        private (bool IsAwaitable, bool ReturnData) DeterminingAwaitable(MethodInfo methodInfo)
        {
            var isAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            if (isAwaitable) 
                return (true, methodInfo.ReturnType.IsGenericType);
            else
                return (false, methodInfo.ReturnType != typeof(void));
        }
    }
}
