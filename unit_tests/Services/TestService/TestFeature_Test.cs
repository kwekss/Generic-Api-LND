using helpers;
using NUnit.Framework;
using System.Threading.Tasks;
using TestService.Features;
using TestService.Models;

namespace unit_tests.Services.TestService
{
    public class TestFeature_Test
    {
        private TestFeature feature;

        [SetUp]
        public void Setup()
        {
            feature = new TestFeature();
            typeof(BaseServiceFeature).GetProperty("Service").SetValue(feature, "Test", null);
            typeof(BaseServiceFeature).GetProperty("FeatureName").SetValue(feature, nameof(TestFeature), null);
        }

        [Test]
        public async Task Will_Return_Exact_Response_From_Feature()
        {
            var input = new TestModel { Prop = "1234" };
            var entryResponse = feature.Entry(input);
            Assert.IsNotNull(entryResponse);
            Assert.IsTrue(entryResponse.Success);
            Assert.AreEqual(entryResponse.ResponseMessage, $"I am {feature.FeatureName} from {feature.Service} and id: {input.Prop}");
        }
    }
}
