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
        public ApiResponse MyEntryMethod(int id)
        {
            return new ApiResponse
            {
                Success = true,
                ResponseMessage = $"I am { FeatureName } from {Service} and id: {id}",
                Data = new DataModel { Prop = "Data" }
            };
        }

    }
}
