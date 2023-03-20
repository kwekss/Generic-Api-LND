using helpers;
using helpers.Atttibutes;
using helpers.Interfaces;
using models;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Features
{
    [ApiDoc(Description = "This is a test endpoint")]
    [Feature(Name = "Test")]
    public class TestFeature : BaseServiceFeature
    {
        private readonly IHttpHelper _httpHelper;

        public TestFeature(IHttpHelper httpHelper) : base()
        {
            _httpHelper = httpHelper;
        }

        [Entry(Method = "POST", Route = "id/{id}")]
        public async Task<ApiResponse> Entry([FromJsonBody] TestModel payload, int id)
        {

            var http = await _httpHelper.ClientBuilder().Url("https://jsonplaceholder.typicode.com/todos/1", "GET").Execute();
            var http2 = await _httpHelper.ClientBuilder()
                .Url("http://localhost:54073/Login/PerformLogin", "POST")
                .AddPayload(new {UserId = "admin", Password = "admin" })
                .Execute();

            return new ApiResponse
            {
                Success = true,
                ResponseMessage = $"I am {FeatureName} from {Service} and id: {payload.Prop}",
                Data = new { payload, apiResponse = http.ToObject<dynamic>() }
            };
        }

    }
}
