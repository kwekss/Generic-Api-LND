using helpers;
using helpers.Atttibutes;
using helpers.Interfaces;
using models;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Features
{
    [ApiDoc(Description = "This is a test endpoint for authentication with integrator")]
    [Feature(Name = "TestIntegrator")]
    public class TestIntegrator : BaseServiceFeature
    {
        private readonly IHttpHelper _httpHelper;

        public TestIntegrator(IHttpHelper httpHelper) : base()
        {
            _httpHelper = httpHelper;
        }

        [Entry(Method = "GET")]
        [Authentication(Schema = AuthenticationType.Integrator, RequestTimeKey = "RequestTime")]
        public async Task<ApiResponse> Entry([FromQuery] TestModel payload)
        {

            var http = await _httpHelper.ClientBuilder().Url("https://jsonplaceholder.typicode.com/todos/1", "GET").Execute();
            
            return new ApiResponse
            {
                Success = true,
                ResponseMessage = $"I am {FeatureName} from {Service} and id: {payload.Prop}",
                Data = new { payload, apiResponse = http.ToObject<dynamic>() }
            };
        }

        [Entry(Method = "POST")]
        [Authentication(Schema = AuthenticationType.Integrator, RequestTimeKey = "RequestTime")]
        public async Task<ApiResponse> EntryPost([FromJsonBody] TestModel payload)
        {

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
