using Newtonsoft.Json;
using System.Collections.Generic;

namespace models
{
    public class OpenApiSpec
    {
        public string openapi { get; set; } = "3.0.3";
        public ApiInfo info { get; set; }
        public List<OpenApiServer> servers { get; set; }
        public object paths { get; set; }
        public dynamic components { get; set; }
        //public string basePath { get; set; }
        //public List<string> schemes { get; set; } = new List<string> { "http", "https" };
        public object definitions { get; set; }
        public ApiInfoExternalDocs externalDocs { get; set; }
    }
    public class OpenApiServer
    {
        public string url { get; set; }
        public OpenApiServerVars variables { get; set; }

    }
    public class OpenApiServerVars
    {
        public OpenApiServerVarsValue hostname { get; set; }
        public OpenApiServerVarsValue protocol { get; set; }
    }
    public class OpenApiServerVarsValue
    {
        public string description { get; set; }
        public string @default { get; set; }
    }
    public class ApiInfo
    {
        public string description { get; set; }
        public string version { get; set; }
        public string title { get; set; }
        public string termsOfService { get; set; }
    }
    public class ApiInfoExternalDocs
    {
        public string description { get; set; }
        public string url { get; set; }
    }
    public class ApiInfoPath
    {
        public string description { get; set; }
        public string summary { get; set; }
        public string operationId { get; set; }
        public List<string> tags { get; set; } = new List<string>();
        //public List<string> consumes { get; set; }
        //public List<string> produces { get; set; }
        public List<ApiInfoPathParameter> parameters { get; set; }
        public dynamic responses { get; set; }
        public dynamic security { get; set; }
        public ApiInfoPathRequestBody requestBody { get; set; }
    }
    public class ApiInfoPathParameter
    {
        public string name { get; set; }
        public string @in { get; set; }
        public string description { get; set; }
        public bool required { get; set; }
        public ApiInfoPathParameterSchema schema { get; set; }
    }
    public class ApiInfoPathParameterSchema
    {
        public string type { get; set; }
        public string @default { get; set; }
    }
    public class ApiInfoPathRequestBody
    {
        public bool required { get; set; }
        public dynamic content { get; set; }
    }
    public class ApiInfoPathRequestBodySchema
    {
        public string type { get; set; }
        public dynamic properties { get; set; }
        public List<string> required { get; set; }
    }
    public class ApiInfoPathRequestObject
    {
        public ApiInfoPathRequestBodySchema schema { get; set; }
        public dynamic example { get; set; }
    }
    public class ApiInfoPathRequestBodyProperty
    {
        public string type { get; set; }
        public string description { get; set; }
        public dynamic @default { get; set; }
        public ApiInfoPathRequestBodyPropertyItem items { get; set; }
        public List<string> @enum { get; set; }
    }
    public class ApiInfoPathRequestBodyPropertyItem
    {
        public string type { get; set; }
        public string description { get; set; }
    }
    public class ApiInfoPathResponse
    {
        public string description { get; set; }
        public dynamic content { get; set; }
        //public ApiInfoPathResponeSchema schema { get; set; }
    }
    public class ApiInfoPathResponeSchema
    {
        [JsonProperty(PropertyName = "$ref")]
        public string @ref { get; set; }
    }
}
