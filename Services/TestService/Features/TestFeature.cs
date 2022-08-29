using helpers;
using helpers.Atttibutes;
using models;
using TestService.Models;

namespace TestService.Features
{
    [Feature(Name = "Test")]
    public class TestFeature : BaseServiceFeature
    {
        public TestFeature() : base()
        {

        }

        [Entry(Method = "POST/GET", Route = "id/{id}")]
        public ApiResponse Entry([FromJsonBody] TestModel payload)
        {
            return new ApiResponse
            {
                Success = true,
                ResponseMessage = $"I am { FeatureName } from {Service} and id: {payload.Prop}",
                Data = payload
            };
        }

    }
}
