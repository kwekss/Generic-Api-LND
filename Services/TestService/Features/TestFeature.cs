using helpers;
using helpers.Atttibutes;
using models;
using TestService.Models;

namespace TestService.Features
{
    [Feature(Name = "TestFeature")]
    public class TestFeature : BaseServiceFeature
    {
        public TestFeature() : base()
        {

        }

        [Entry(Method = "POST/GET", Route = "id/{id}")]
        public ApiResponse MyEntryMethod([FromJsonBody] TestModel input,[FromQuery] TestModel input2, int id)
        {
            return new ApiResponse { Success = true, ResponseMessage = $"I am { FeatureName } from {Service} and from body {input.Prop} and id: {id}" };
        }
        
    }
}
