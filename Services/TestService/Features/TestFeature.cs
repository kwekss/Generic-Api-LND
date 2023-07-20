using helpers;
using helpers.Atttibutes;
using helpers.Exceptions;
using helpers.Interfaces;
using Microsoft.Extensions.Configuration;
using models;
using System;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Features
{
    [ApiDoc(Description = "This is a test endpoint")]
    [Feature(Name = "Test")]
    public class TestFeature : BaseServiceFeature
    {
        private readonly IHttpHelper _httpHelper;

        public TestFeature(IHttpHelper httpHelper, IConfiguration config) : base()
        {
            _httpHelper = httpHelper;
        }

        [Entry(Method = "POST", Route = "id/{id}")]
        //[Authentication(Schema = AuthenticationType.Integrator, RequestTimeKey = "RequestTime")]
        public async Task<ApiResponse> Entry([FromJsonBody] TestModel payload, int id)
        {

            throw new ApiRequestStatusException(401, "Request ID is required");
            var http = await _httpHelper.ClientBuilder().Url("https://jsonplaceholder.typicode.com/todos/1", "GET").Execute();
            
            return new ApiResponse
            {
                Success = true,
                ResponseMessage = $"I am {FeatureName} from {Service} and id: {payload.Prop}",
                Data = new { payload, apiResponse = http.ToObject<dynamic>() }
            };
        }

    }
}
