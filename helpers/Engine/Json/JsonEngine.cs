using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace helpers.Engine.Json
{
    public class JsonEngine
    {
        public delegate dynamic BindingFunction(params dynamic[] args); 
        private readonly string _pattern = @"@(\w+)\(([^)]*)\)|\$\.[\w.]+";
        private List<KeyValuePair<string, MethodInfo>> _methods;
        public JsonEngine()
        { 
            _methods = new List<KeyValuePair<string, MethodInfo>>();
            RegisterClass(typeof(InternalFunctions));
        }

        public string Transform(string jsonTemplate, dynamic data)
        {
            string modifiedJson = ReplaceJsonPathExpressions(jsonTemplate, data);
            return modifiedJson;
        }

        public JsonEngine RegisterFunction(string name, MethodInfo method)
        {
            _methods.Add(new KeyValuePair<string, MethodInfo>(name, method));

            return this;
        }

        public JsonEngine RegisterFunction(string name, BindingFunction fn)
        {
            MethodInfo method = fn.GetMethodInfo();
            _methods.Add(new KeyValuePair<string, MethodInfo>(name, method));

            return this;
        }

        public JsonEngine RegisterClass(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                _methods.Add(new KeyValuePair<string, MethodInfo>(method.Name, method));
            }
            return this;
        }


        private string ReplaceJsonPathExpressions(string jsonString, dynamic data)
        {
            string replacedJson = Regex.Replace(jsonString, _pattern, match =>
            {
                // Check if it's a function with '@' prefix
                if (match.Value.StartsWith("@"))
                {
                    string functionName = match.Groups[1].Value;
                    string functionArgs = match.Groups[2].Value;

                    dynamic[] args = functionArgs.Replace(" ", "").Split(new string[] { ",", ", " }, StringSplitOptions.None).Select(arg =>
                    {
                        if (!arg.StartsWith("$"))
                        {
                            arg = arg.TrimStart('"');
                            arg = arg.TrimEnd('"');
                            arg = arg.TrimStart('\'');
                            arg = arg.TrimEnd('\'');
                            return arg;
                        }
                        return GetValueFromJsonPath(data, arg.Trim());
                    }).ToArray();

                    var method = _methods.FirstOrDefault(x => x.Key == functionName).Value;
                    if (method == null) return match.Value;

                    object instance = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                    List<dynamic> fnParams = new List<dynamic>();

                    ParameterInfo[] parameters = method.GetParameters();

                    int paramIndex = 0;

                    foreach (ParameterInfo paramInfo in parameters)
                    {
                        if (paramInfo.GetCustomAttributes(typeof(ParamArrayAttribute), false).Any() || paramInfo.ParameterType == typeof(object[]))
                        {
                            dynamic[] remainingArgs = args.Skip(paramIndex).ToArray();
                            fnParams.Add(remainingArgs);
                            break;
                        }
                        else if (paramIndex < args.Length)
                        {
                            fnParams.Add(args[paramIndex]);
                            paramIndex++;
                        }
                        else if (paramInfo.IsOptional)
                        {
                            fnParams.Add(Type.Missing);
                        }
                        else
                        {

                        }
                    }

                    object result = method.Invoke(instance, fnParams.ToArray());
                    if(string.IsNullOrWhiteSpace($"{result}")) return match.Value;

                    return result?.ToString();
                }

                string jsonPath = match.Value;

                dynamic result2 = GetValueFromJsonPath(data, jsonPath);
                return result2.ToString();

            });

            return FinalModifier(replacedJson);
        }

        private string FinalModifier(string jsonString)
        {
            return jsonString
                .Replace("\"(", "")
                .Replace(")\"", "")
                ;
        }
        private dynamic EvaluateFunction(dynamic data, string functionName, string[] args)
        {
            switch (functionName.ToLower())
            {
                case "length":
                    // Get the value from the JSONPath expression
                    dynamic value = GetValueFromJsonPath(data, args[0]);

                    // Check if the value is a string and return its length
                    if (value is string strValue)
                    {
                        return strValue.Length;
                    }
                    break;
                case "sum":
                    // Evaluate the sum function with multiple arguments
                    int sum = 0;
                    foreach (string arg in args)
                    {
                        dynamic argValue = GetValueFromJsonPath(data, arg);
                        if (argValue is int || argValue is long)
                        {
                            sum += argValue;
                        }
                    }
                    return sum;
                    // Add more functions as needed
            }

            // If the function is not supported or the value is not a string, return null
            return null;
        }

        private dynamic GetValueFromJsonPath(dynamic data, string jsonPath)
        {
            // Split the JSONPath into segments
            string token = jsonPath;

            JObject payload = JObject.FromObject(data);

            var pathValue = payload.SelectToken(token);

            return EvalType(pathValue);
        }

        private dynamic EvalType(JToken value)
        {
            if (value == null) return null;
            switch (value.Type)
            {
                case JTokenType.None:
                case JTokenType.Object:
                case JTokenType.Array:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Integer:
                    long.TryParse(value?.ToString(), out long intVal);
                    return intVal;
                case JTokenType.Float:
                    float.TryParse(value?.ToString(), out float floatVal);
                    return floatVal;
                case JTokenType.String:
                    return value?.ToString();
                case JTokenType.Boolean:
                    bool.TryParse(value?.ToString(), out bool boolVal);
                    return boolVal;
                case JTokenType.Null:
                    return null;
                case JTokenType.Undefined:
                    return null;
                case JTokenType.Date:
                    DateTime.TryParse(value?.ToString(), out var dateVal);
                    return dateVal;
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                    Guid.TryParse(value?.ToString(), out var guidVal);
                    return guidVal;
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                default:
                    return value?.ToString();
            }
        }
    }
}
