using helpers.Atttibutes;
using helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static helpers.Atttibutes.AuthenticationAttribute;

namespace helpers.Engine
{
    public class DocumentationBuilder : IDocumentationBuilder
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _url_prefix;
        private readonly TextInfo _textInfo;

        public DocumentationBuilder(IConfiguration config, IHttpContextAccessor httpContextAccessor, IServiceProvider services)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = services;
            _url_prefix = config["URL_PREFIX"] ?? "";
            _textInfo = CultureInfo.CurrentCulture.TextInfo;
        }

        public async Task RenderOpenApiSpecs(string name = null)
        {
            _httpContextAccessor.HttpContext.Response.Headers["content-type"] = "application/json";


            var projectName = ProjectName(name);

            string host = $"{_httpContextAccessor.HttpContext.Request.Host}";
            JObject paths = new JObject();
            JObject components = new JObject();

            components["schemas"] = new JObject();
            components["securitySchemes"] = new JObject();

            var service = _serviceProvider.GetServices<BaseServiceFeature>()
                   .Where(_ => _.GetType().GetCustomAttributes().Any(_ => _.GetType() == typeof(FeatureAttribute)
                    )).ToList();

            for (int i = 0; i < service.Count; i++)
            {
                var serviceType = service[i].GetType();
                var featureAttr = (FeatureAttribute)serviceType.GetCustomAttribute(typeof(FeatureAttribute));
                var docsAttr = (ApiDocAttribute)serviceType.GetCustomAttribute(typeof(ApiDocAttribute));

                var serviceName = serviceType.Assembly.ManifestModule.ScopeName.Replace("Service.dll", "");



                var route = serviceType.GetMethods();
                var entries = route.Where(x => x.GetCustomAttributes(true).Any(_ => _.GetType() == typeof(EntryAttribute))).ToList();

                var bearerAuths = entries.Any(x => (x.GetCustomAttribute(typeof(AuthenticationAttribute)) as AuthenticationAttribute)?.Schema == AuthenticationType.InternalBearerToken);
                if (bearerAuths)
                    components["securitySchemes"]["bearerAuth"] = JObject.FromObject(new { required= true, type = "http", name = "Authorization", scheme = "bearer", @in = "header" });

                for (int j = 0; j < entries.Count; j++)
                {
                    var path = $"/{serviceName}/{featureAttr.Name}";
                    var entryType = entries[j];
                    var entryAttr = entryType.GetCustomAttribute(typeof(EntryAttribute)) as EntryAttribute;
                    var apiDocAttr = entryType.GetCustomAttribute(typeof(ApiDocAttribute)) as ApiDocAttribute;

                    var apiPathInfo = new ApiInfoPath
                    {
                        tags = new List<string> { },
                        summary = $"{docsAttr?.Description ?? featureAttr.Name}",
                        description = apiDocAttr?.Description,
                        operationId = featureAttr.Name,
                        parameters = new List<ApiInfoPathParameter> { },
                        responses = new JObject(),
                        security = new JObject()
                    }; 

                    var resp200 = new ApiInfoPathResponse { description = "", content = new JObject() };

                    var responseObject = new
                    {
                        schema = new
                        {
                            type = "object",
                            properties = new JObject(),
                        },
                    };

                    var response = entryType.ReturnType.GetProperties().FirstOrDefault();
                    var schema = new ApiInfoPathRequestBodySchema
                    {
                        type = "object",
                        properties = new JObject(),
                    };
                    var props = response.PropertyType.GetProperties();
                    for (int k = 0; k < props.Length; k++)
                    {
                        var prop = props[k];
                        var propName = toCamelCase(prop.Name);
                        var property = new
                        {
                            type = getParameterType(prop.PropertyType),
                        };
                        schema.properties[propName] = JObject.FromObject(property);
                    }
                    components["schemas"][response.PropertyType.Name] = JObject.FromObject(schema);


                    var p = new JObject();
                    p["$ref"] = $"#/components/schemas/{response.PropertyType.Name}";

                    resp200.content["application/json"] = JObject.FromObject(new { schema = JObject.FromObject(p) });
                    apiPathInfo.responses["200"] = JObject.FromObject(resp200);

                    var methods = entryAttr.Method.Split("/").ToList();
                    path = $"{path}{(!string.IsNullOrWhiteSpace(entryAttr.Route) ? $"/{entryAttr.Route.Trim('/')}" : "")}";
                    paths[path] = new JObject();

                    var entryParameters = entryType.GetParameters()?.ToList();

                    var parameters = GetParameters(entryType, entryParameters, path);
                    apiPathInfo.parameters.AddRange(parameters);

                    var requestPayload = entryParameters.Where(x => x.GetCustomAttribute(typeof(FromJsonBodyAttribute)) != null)?.ToList();
                    if (requestPayload != null && requestPayload.Any())
                    {
                        apiPathInfo.requestBody = GetRequestPayload(requestPayload, components);
                    }

                    for (int k = 0; k < methods.Count; k++)
                    {
                        apiPathInfo.tags.Add(serviceName);
                        var method = methods[k].Trim().ToLower();
                        apiPathInfo.operationId = $"service/{serviceName}/{featureAttr.Name}{(methods.Count > 1 ? $"/{method}" : "")}".ToLower();
                        paths[path][methods[k].Trim().ToLower()] = JObject.FromObject(apiPathInfo);
                    }

                }
            }


            var specs = new OpenApiSpec
            {
                servers = new List<OpenApiServer> {
                    new OpenApiServer
                    {
                        url = "{protocol}://{hostname}" + $"{_url_prefix}",
                        variables = new OpenApiServerVars
                        {
                            hostname = new OpenApiServerVarsValue
                            {
                                description = $"{projectName} API server or host",
                                @default = host
                            },
                            protocol = new OpenApiServerVarsValue
                            {
                                description = $"{projectName} API server or protocol",
                                @default = "http"
                            }
                        }
                    }
                },
                info = new ApiInfo
                {
                    version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    description = $"API documentation for {projectName}",
                    title = projectName
                },
                paths = paths,
                components = components,
                definitions = new { },
                externalDocs = new ApiInfoExternalDocs { description = "", url = "" }
            };

            await _httpContextAccessor.HttpContext.Response.WriteAsync(specs.Stringify());
        }

        private ApiInfoPathRequestBody GetRequestPayload(List<ParameterInfo> requestPayload, JObject components)
        {
            var body = new ApiInfoPathRequestBody
            {
                required = true,
                content = new JObject()
            };

            var requestObject = new
            {
                schema = JObject.FromObject(new
                {
                    type = "object",
                    properties = new JObject(),
                    required = new List<string> { }
                })

            };

            for (int i = 0; i < requestPayload.Count; i++)
            {
                var pl = requestPayload[i].ParameterType.GetProperties().ToList();
                for (int j = 0; j < pl.Count; j++)
                {
                    var docsAttr = (ApiDocAttribute)pl[j].GetCustomAttribute(typeof(ApiDocAttribute));
                    if (pl[j].PropertyType.IsClass && !pl[j].PropertyType.FullName.StartsWith("System."))
                    {
                        createSchema(pl[j], components);
                        var prop = pl[j];
                        var propName = toCamelCase(prop.Name);
                        var property = JObject.FromObject(new
                        {
                            type = prop.PropertyType.Name,
                        });
                        //requestObject.schema["type"] = prop.PropertyType.Name;
                        property["$ref"] = $"#/components/schemas/{prop.PropertyType.Name}";
                        requestObject.schema["properties"][propName] = JObject.FromObject(property);
                    }
                    else
                    {
                        var props = new
                        {
                            type = getParameterType(pl[j].PropertyType),
                            description = docsAttr?.Description,
                            @default = docsAttr?.Default,
                        };
                        requestObject.schema["properties"][toCamelCase(pl[j].Name)] = JObject.FromObject(props);
                    }

                }
            }

            body.content["application/json"] = JObject.FromObject(requestObject);

            return body;
        }

        private void createSchema(PropertyInfo propertyInfo, JObject components)
        {
            var schema = new ApiInfoPathRequestBodySchema
            {
                type = "object",
                properties = new JObject(),
            };

            var props = propertyInfo.PropertyType.GetProperties();
            for (int k = 0; k < props.Length; k++)
            {
                if (props[k].PropertyType.IsClass && !props[k].PropertyType.FullName.StartsWith("System."))
                {
                    createSchema(props[k], components);
                    var prop = props[k];
                    var propName = toCamelCase(prop.Name);
                    var property = JObject.FromObject(new
                    {
                        type = prop.PropertyType.Name,
                    });
                    //schema.type = prop.PropertyType.Name;
                    property["$ref"] = $"#/components/schemas/{prop.PropertyType.Name}";
                    schema.properties[propName] = JObject.FromObject(property);
                }
                else
                {
                    var prop = props[k];
                    var propName = toCamelCase(prop.Name);
                    var property = new
                    {
                        type = getParameterType(prop.PropertyType),
                    };
                    schema.properties[propName] = JObject.FromObject(property);
                }

            }
            components["schemas"][propertyInfo.PropertyType.Name] = JObject.FromObject(schema);
        }

        private List<ApiInfoPathParameter> GetParameters(MethodInfo entryType, List<ParameterInfo> parameterInfos, string path)
        {
            var payloads = new List<ApiInfoPathParameter>();
            for (int i = 0; i < parameterInfos.Count; i++)
            {
                var parameterInfo = parameterInfos[i];
                if (parameterInfo.GetCustomAttribute(typeof(FromQueryAttribute)) != null)
                {
                    var properties = parameterInfo.ParameterType.GetProperties();
                    for (int j = 0; j < properties.Length; j++)
                    {
                        var payload = new ApiInfoPathParameter
                        {
                            @in = "query",
                            name = properties[j].Name,
                            schema = new ApiInfoPathParameterSchema
                            {
                                type = getParameterType(parameterInfo.ParameterType),
                                @default = ""
                            }
                        };

                        payloads.Add(payload);
                    }
                }
                else
                {
                    var sysParamTypes = new string[] { "System.String" };
                    if (!parameterInfo.ParameterType.IsClass || sysParamTypes.Contains(parameterInfo.ParameterType.FullName))
                    {
                        var source = findPayloadSource(parameterInfo, path);
                        var payload = new ApiInfoPathParameter
                        {
                            @in = source,
                            name = parameterInfo.Name,
                            schema = new ApiInfoPathParameterSchema
                            {
                                type = getParameterType(parameterInfo.ParameterType),
                                @default = ""
                            },
                            required = (new string[] { "path" }).Contains(source)
                        };

                        payloads.Add(payload);
                    }

                }

            }

            var authAttr = entryType.GetCustomAttribute(typeof(AuthenticationAttribute)) as AuthenticationAttribute;
            if (authAttr?.Schema == AuthenticationType.InternalBearerToken)
            { 
                payloads.Add(new ApiInfoPathParameter
                {
                    description = "Bearer Token",
                    @in = "header",
                    name = "Authorization",
                    required = true,
                    schema = new ApiInfoPathParameterSchema { type = "string", @default = $"Bearer {Guid.NewGuid()}" }
                });
            }

            return payloads;
        }

        private string findPayloadSource(ParameterInfo parameterInfo, string path)
        {
            if (parameterInfo.GetCustomAttribute(typeof(FromJsonBodyAttribute)) != null) return "body";
            if (path.Contains($"{{{parameterInfo.Name}}}")) return "path";
            return "query";
        }
        private string getParameterType(Type propInfo)
        {
            if (propInfo.Name.ToLower().StartsWith("int")) return "integer";
            return toCamelCase(propInfo.Name);
        }
        private string getParameterFormat(Type propInfo)
        {
            return toCamelCase(propInfo.Name);
        }

        public async Task RenderView(string name = null)
        {
            _httpContextAccessor.HttpContext.Response.Headers["content-type"] = "text/html";
            var html = "<!doctype html> <html lang=\"en\"> " +
                "<head> " +
                "<meta charset=\"utf-8\"> " +
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=no\"> " +
                "<title>$$name$$</title> " +
                "<!-- Elements: Web Component --> " +
                "<script src=\"https://unpkg.com/@stoplight/elements/web-components.min.js\"></script> " +
                "<link rel=\"stylesheet\" href=\"https://unpkg.com/@stoplight/elements/styles.min.css\"> " +
                "<style> body { display: flex; flex-direction: column; height: 100vh; } main { flex: 1 0 0; overflow: hidden; } </style> " +
                "</head> " +
                "<body> " +
                "<main role=\"main\"> " +
                "<elements-api apiDescriptionUrl=\"$$url_prefix$$/api-docs/specs\" router=\"hash\" /> </main> " +
                "</body> " +
                "</html>"
                .Replace("$$name$$", ProjectName(name))
                .Replace("$$url_prefix$$", _url_prefix);

            await _httpContextAccessor.HttpContext.Response.WriteAsync(html);
        }



        private string ProjectName(string projectName)
        {
            var SolutionFullPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName.Replace("\\","/");
            var tempStrings = SolutionFullPath.Split('/');
            return _textInfo.ToTitleCase($"{tempStrings[tempStrings.Length - 2].Replace("_", " ").Replace("-", " ")} - {projectName.Replace("_", " ").Replace("-", " ")} ");
        }


        public static string toCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > 1)
            {
                return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
            return str.ToLowerInvariant();
        }
    }
}
