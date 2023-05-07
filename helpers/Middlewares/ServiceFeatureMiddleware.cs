﻿using helpers.Atttibutes;
using helpers.Exceptions;
using helpers.Interfaces;
using helpers.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using models;
using MongoDB.Driver.Core.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace helpers.Middlewares
{
    public class ServiceFeatureMiddleware
    {
        private readonly IDocumentationBuilder _documentationBuilder;
        private readonly IMessengerHub _messengerHub;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private readonly RequestDelegate _next;
        private readonly bool _is_logging_enabled;
        private readonly bool _is_api_doc_enabled;
        private readonly string _url_prefix;
        private readonly string _api_type;
        private readonly JsonSerializerSettings _serializerSettings;

        public ServiceFeatureMiddleware(IDocumentationBuilder documentationBuilder, IMessengerHub messengerHub, IServiceProvider serviceProvider, IConfiguration config, RequestDelegate next)
        {
            _documentationBuilder = documentationBuilder;
            _messengerHub = messengerHub;
            _serviceProvider = serviceProvider;
            _config = config;
            _next = next;
            _is_logging_enabled = Convert.ToBoolean(config["ENABLE_LOGGING"] ?? "false");
            _is_api_doc_enabled = Convert.ToBoolean(config["ENABLE_API_DOCS"] ?? "false");
            _api_type = config["API_TYPE"] ?? "WEB_API";
            _url_prefix = config["URL_PREFIX"] ?? "";

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

#if DEBUG
            _is_api_doc_enabled = true;
#endif
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                LogContext.PushProperty("CorrelationId", GetCorrelationId(context));

                var projectName = AppDomain.CurrentDomain.FriendlyName;

                var urlPath = context.Request.Path.Value.ToLower();
                if (!string.IsNullOrWhiteSpace(_url_prefix))
                {
                    if (!urlPath.StartsWith(_url_prefix.ToLower()))
                    {
                        await Respond(context, "Invalid URL provided.");
                        return;
                    }
                    var regex = new Regex(Regex.Escape(_url_prefix.ToLower()));
                    urlPath = regex.Replace(urlPath, "", 1).Trim('/');
                    //urlPath = urlPath.Replace(_url_prefix.ToLower(), "", 1);
                }

                if (_is_api_doc_enabled && urlPath.Trim('/').ToLower().StartsWith("api-docs"))
                {
                    if (urlPath.ToLower().EndsWith("specs"))
                    {
                        await _documentationBuilder.RenderOpenApiSpecs(projectName);
                    }
                    else
                    {
                        await _documentationBuilder.RenderView(projectName);
                    }
                    return;
                }

                Log.Information($"Incoming Request Method: {context.Request.Method}");

                var path = urlPath.Split('/').Where(_ => !string.IsNullOrWhiteSpace(_)).ToList();
                if (path.Count < 2)
                {
                    await Respond(context, "Service or feature not found");
                    return;
                }



                var endpoint = new ServiceEndpoint(path);
                if (context.Request.Method.ToLower() != "get" && context.Request.ContentType != null && context.Request.ContentType.Contains("multipart/form-data") && (context.Request?.Form?.Files?.Any() ?? false))
                {
                    endpoint.Files = context.Request.Form.Files.Select((f) =>
                    {
                        byte[] bytes;
                        using (var ms = new MemoryStream())
                        {
                            f.CopyToAsync(ms).Wait();
                            bytes = ms.ToArray();
                        }
                        return new FileContent { Name = f.Name, Content = bytes, FileName = f.FileName, FileSize = f.Length, MimeType = f.ContentType };
                    }).ToList();

                }


                var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
                await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                endpoint.RequestBody = buffer;

                if (endpoint.Service.ToLower().StartsWith("assets"))
                {
                    await _next.Invoke(context);
                    return;
                }

                if (endpoint.RequestBody.Length > 0)
                {
                    Log.Information($"Incoming Request Payload: {Encoding.UTF8.GetString(endpoint.RequestBody)}");
                }

                if (context.Request.QueryString.HasValue)
                    Log.Information($"Incoming Request Query: {context.Request.QueryString.ToUriComponent()}");

                var service = _serviceProvider.GetServices<BaseServiceFeature>()
                    .FirstOrDefault(_ => _.GetType().GetCustomAttributes().Any(_ => _.GetType() == typeof(FeatureAttribute)
                     && (_ as FeatureAttribute).Name.ToLower() == endpoint.Feature.ToLower()) &&
                    _.GetType().Namespace.ToLower() == endpoint.RequiredFullname.ToLower());


                if (service == null)
                {
                    await Respond(context, "Service not found");
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
                var feature = (FeatureAttribute)service.GetType().GetCustomAttribute(typeof(FeatureAttribute));

                RouteRegex regexRoute = null;
                if (!string.IsNullOrWhiteSpace(entry.Route))
                {
                    regexRoute = ConvertRouteToRegex(entry.Route.ToLower());
                    var regex = Regex.Matches(endpoint.Route.ToLower(), regexRoute.Regex);
                    if (regex.Count == 0)
                    {
                        await Respond(context, "Route not found");
                        return;
                    }
                    if (regex.Count > 0)
                    {
                        regexRoute.RouteParams.ForEach(_ =>
                        {
                            _.Value = regex[0].Groups[_.Index].Value;
                        });
                    }
                }


                var state = DeterminingAwaitable(featureEntry);

                InvokeEntryAttributes(service, context, endpoint, featureEntry);

                object featureResponse = await InvokeFeatureEntry(context, endpoint, service, featureEntry, state, regexRoute);
                var responseContent = feature.ContentType.ToLower().Contains("json") ? featureResponse.Stringify(_serializerSettings) : featureResponse;
                 
                await WriteResponse(context, responseContent, 200, feature.ContentType);

                return;
            }
            catch (ParameterException e)
            {
                await Respond(context, e.Message);
                return;
            }
            catch (ApiRequestStatusException e)
            {
                var data = e.Message.Split("||");
                var statusCode = Convert.ToInt32(data[0]);
                Log.Information($"Response: {statusCode} => {data[1]}");

                await Respond(context, data[1], statusCode);
                return;
            }
            catch (CustomException e)
            {
                Log.Error(e.ToString());
                await Respond(context, e.Message);
                return;
            }
            catch (WarningException e)
            {
                Log.Error(e.ToString());
                await Respond(context, e.Message);
                return;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                await Respond(context, $"A system error occured. Please try again");
                return;
            }
        }

        private async Task WriteResponse(HttpContext context, object responseContent, int responseCode, string contentType)
        {
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {

                using var writer = new StreamWriter(responseBody);
                writer.WriteLine(responseContent);
                writer.Flush();
                responseBody.Position = 0;

                context.Response.Body = responseBody;
                long length = 0;
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.ContentLength = length;
                    context.Response.StatusCode = responseCode;
                    context.Response.Headers["content-type"] = contentType;

                    return Task.CompletedTask;
                });

                await _next(context);

                length = context.Response.Body.Length;
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
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
                        routeRegex.RouteParams.Add(new RouteRegexParam { Index = i + 1, Name = match.Groups[1].Value, Type = match.Groups[2].Value });
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

        private void InvokeEntryAttributes(BaseServiceFeature service, HttpContext context, ServiceEndpoint endpoint, MethodInfo entry)
        {
            SetFeatureDefaultValues(context, endpoint, service);
            var attributes = entry.GetCustomAttributes(true).ToList();
            for (int j = 0; j < attributes.Count; j++)
            {
                if (attributes[j] is IEntryAttribute)
                {
                    (attributes[j] as IEntryAttribute).InitAttribute(context, endpoint, _serviceProvider);
                }
            }
            SetFeatureDefaultValues(context, endpoint, service);
        }

        private async Task<object> InvokeFeatureEntry(HttpContext context, ServiceEndpoint endpoint, BaseServiceFeature service, MethodInfo matchingFeature, (bool IsAwaitable, bool ReturnData) state, RouteRegex routeRegex)
        {
            object featureResponse = new { };
            if (state.IsAwaitable && state.ReturnData)
                featureResponse = await (dynamic)matchingFeature.Invoke(service, PopulateParameters(matchingFeature.GetParameters(), endpoint, context, routeRegex));

            if (state.IsAwaitable && !state.ReturnData)
                await (dynamic)matchingFeature.Invoke(service, null);

            if (!state.IsAwaitable && state.ReturnData)
                featureResponse = matchingFeature.Invoke(service, PopulateParameters(matchingFeature.GetParameters(), endpoint, context, routeRegex));

            if (!state.IsAwaitable && !state.ReturnData)
                matchingFeature.Invoke(service, null);

            return featureResponse;
        }

        private static void SetFeatureDefaultValues(HttpContext context, ServiceEndpoint endpoint, BaseServiceFeature service)
        {
            typeof(BaseServiceFeature).GetProperty("Service").SetValue(service, endpoint.Service, null);
            typeof(BaseServiceFeature).GetProperty("FeatureName").SetValue(service, endpoint.Feature, null);
            typeof(BaseServiceFeature).GetProperty("ServiceEndpoint").SetValue(service, endpoint, null);
            typeof(BaseServiceFeature).GetProperty("Context").SetValue(service, context, null);
        }

        private async Task Respond(HttpContext context, string message, int statusCode = 200)
        {
            context.Response.StatusCode = statusCode;
            if (_api_type == "USSD_API")
            { 
                var response = new UssdApiResponse { ResponseBody = message }.Stringify(_serializerSettings); 
                await WriteResponse(context, response, statusCode, "application/json");
            }
            if (_api_type == "WEB_API")
            { 
                var response = new ApiResponse { ResponseMessage = message }.Stringify(_serializerSettings);
                await WriteResponse(context, response, statusCode, "application/json");
            }
            else
            {
                context.Response.Headers.ContentLength = message.Length;
                await WriteResponse(context, message, statusCode, "text/plain");
            }
        }

        private object[] PopulateParameters(IEnumerable<ParameterInfo> parameters, ServiceEndpoint endpoint, HttpContext httpContext, RouteRegex routeRegex)
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
                            (attributes[j] as IParameterAttribute).InitAttribute(parameter, httpContext, endpoint, p);
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
                throw new ParameterException(e.Message);
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

        public string GetCorrelationId(HttpContext httpContext)
        {
            httpContext.Request.Headers.TryGetValue("Cko-Correlation-Id", out StringValues correlationId);
            return correlationId.FirstOrDefault() ?? httpContext.TraceIdentifier;
        }
    }
}
