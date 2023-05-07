﻿using helpers.Exceptions;
using helpers.Interfaces;
using helpers.Notifications;
using helpers.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthenticationAttribute : Attribute, IEntryAttribute
    {
        
        public AuthenticationType Schema { get; set; }
        public string RequestTimeKey { get; set; }
        public AuthenticationAttribute()
        {

        }

        public void InitAttribute(HttpContext context, ServiceEndpoint endpoint, IServiceProvider services)
        {
            if (Schema == AuthenticationType.Integrator)
                ValidateIntegrator(context, services, endpoint);

            if (Schema == AuthenticationType.InternalBearerToken)
                AuthenticateInternalBearerToken(context, services, endpoint);

            if (Schema == AuthenticationType.OpenIdToken)
                ValidateIdentityServerToken(context, services, endpoint);
        }

        private void AuthenticateInternalBearerToken(HttpContext context, IServiceProvider services, ServiceEndpoint endpoint)
        {
            var _sessionManager = services.GetService<ISessionManager>();
            var session = _sessionManager.GetCurrentUserSession().Result;

            if (session == null) throw new ApiRequestStatusException(401, "Request authorization failed. Please check and try again");

            if (string.IsNullOrWhiteSpace(session.Data)) throw new ApiRequestStatusException(401, "Request authorization failed. Please check and try again");

            var sessionData = session.Data.ParseObject<AuthorizedUser>();
            if (sessionData.TokenExpiry < DateTime.Now)
            {
                _sessionManager.DeleteSessionData(session.SessionKey).Wait();
                throw new ApiRequestStatusException(401, "Authorization token expired. Kindly login to continue");
            }
            endpoint.AuthenticationType = AuthenticationType.InternalBearerToken;
        }
        private void ValidateIdentityServerToken(HttpContext context, IServiceProvider services, ServiceEndpoint endpoint)
        {
            var config  = services.GetService<IConfiguration>();
            var isAuthEnabled = config.GetValue("Utility:Authentication:IdentityServer:Enabled", true);
            var authorityEndpoint = config.GetValue("Utility:Authentication:IdentityServer:Authority", "");
            var audience = config.GetSection("Utility:Authentication:IdentityServer:Audience").Get<List<string>>() ?? new List<string>();
            
            if (!isAuthEnabled) return;

            var authorizationString = context.Request.Headers["Authorization"].ToString();
            Log.Information($"Auth String: {authorizationString}");

            if (string.IsNullOrEmpty(authorizationString)) throw new ApiRequestStatusException(401, "Invalid Authorization token");

            authorizationString = authorizationString.Trim();
            string[] authorizationArray = authorizationString.Split(Convert.ToChar(" "));
            if (!authorizationArray.Any()) throw new ApiRequestStatusException(401, "Invalid Authorization token");
            if (authorizationArray.Length > 2) throw new ApiRequestStatusException(401, "Invalid Authorization token");
            if (authorizationArray.Length < 2) throw new ApiRequestStatusException(401, "Invalid Authorization token");

            var schema = authorizationArray[0];
            var token = authorizationArray[1];

           
            var openIdConfigurationEndpoint = $"{authorityEndpoint}/.well-known/openid-configuration";
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigurationEndpoint, new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = authorityEndpoint,
                ValidAudiences = audience,
                IssuerSigningKeys = openIdConfig.SigningKeys
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            try
            {
                var user = handler.ValidateToken(token, validationParameters, out var validatedToken);
                if (!user.Identity.IsAuthenticated || DateTime.Now > validatedToken?.ValidTo)
                    throw new ApiRequestStatusException(401, "Authorization failed. Kindly try again");
                
                endpoint.User = user;
                endpoint.AuthenticationType = AuthenticationType.OpenIdToken;
            }
            catch (Exception e)
            {
                Log.Error($"Auth Error: {e}");
                throw new ApiRequestStatusException(401, "Authorization failed.");
            }
            

        }
        private void ValidateIntegrator(HttpContext context, IServiceProvider services, ServiceEndpoint endpoint)
        {
            var config  = services.GetService<IConfiguration>();
            var isIntegratorAuthEnabled = config.GetValue("Utility:Authentication:Integrator:Enabled", true);
            if (!isIntegratorAuthEnabled) return;

            var authorizationString = context.Request.Headers["Authorization"].ToString();
            var payloadObj = endpoint.RequestBodyToObject();

            if (payloadObj == null) throw new CustomException($"Invalid request payload");
            JObject payload = JObject.FromObject(payloadObj);

            var pathValue = payload.SelectToken(string.IsNullOrWhiteSpace(RequestTimeKey) ? "RequestTime" : RequestTimeKey);

            if (string.IsNullOrWhiteSpace(pathValue?.ToString())) throw new CustomException($"Request timestamp must be set");

            DateTime requestTime = Convert.ToDateTime(pathValue.ToString());

            var integrator = services.GetService<IIntegratorHelper>();
            Log.Information($"Auth String: {authorizationString}");

            if (integrator != null)
            {
                var validateRequest = integrator.ValidateIntegrator(authorizationString, requestTime).Result;
                if (!string.IsNullOrWhiteSpace(validateRequest))
                    throw new CustomException($"{validateRequest}");

                endpoint.AuthenticationType = AuthenticationType.Integrator;
            }
            else
            {
                throw new CustomException("An error occured. Please try again");
            }
        }
    }




}
