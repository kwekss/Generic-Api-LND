using helpers;
using helpers.Atttibutes;
using helpers.Interfaces;
using models;
using System.Linq;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Features
{
    [ApiDoc(Description = "Testing OpenId Token")]
    [Feature(Name = "TestOpenIdToekn")]
    public class TestOpenIdToken : BaseServiceFeature
    {
        private readonly IHttpHelper _httpHelper;

        public TestOpenIdToken(IHttpHelper httpHelper) : base()
        {
            _httpHelper = httpHelper;
        }

        [Entry(Method = "POST")]
        [Authentication(Schema = AuthenticationType.OpenIdToken)]
        public async Task<ApiResponse> Entry([FromJsonBody] TestModel payload)
        {
            var claim = ServiceEndpoint.User.Claims.FirstOrDefault(x => x.Type == "client_id");
            if (claim == null) return new ApiResponse { ResponseMessage = "Authorization failed."}; 

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
