using helpers;
using helpers.Engine;
using helpers.Engine.Json;
using helpers.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SharpCompress.Common;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TestService.Features;
using TestService.Models;
using static MongoDB.Driver.WriteConcern;

namespace unit_tests.Services.TestService
{
    public class TestFeature_Test
    {
        private TestFeature feature;

        [SetUp]
        public void Setup()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpHelperMock = new Mock<HttpHelper>();

            //feature = new TestFeature(httpHelperMock.Object);
            //typeof(BaseServiceFeature).GetProperty("Service").SetValue(feature, "Test", null);
            //typeof(BaseServiceFeature).GetProperty("FeatureName").SetValue(feature, nameof(TestFeature), null);
        }

        //[Test]
        //public async Task Will_Return_Exact_Response_From_Feature()
        //{
        //    var input = new TestModel { Prop = "1234" };
        //    var entryResponse = feature.Entry(input);
        //    Assert.IsNotNull(entryResponse);
        //    Assert.IsTrue(entryResponse.Success);
        //    Assert.AreEqual(entryResponse.ResponseMessage, $"I am {feature.FeatureName} from {feature.Service} and id: {input.Prop}");
        //}

        [Test]
        public async Task Will_mask_passwords()
        {
            var message = Regex.Replace("{\"token\": \"1234\",\"try\":\"this123\",\"try2\":\"this123\"}", "\"(Token)\":\\s?(\"(.*?)\")", "\"$1\": \"*****\"", RegexOptions.IgnoreCase);
            Console.WriteLine(message);

            message = Regex.Replace("{\"password\":\"1234\"}", "\"(password)\":\\s?(\"(.*?)\")", "\"$1\": \"*****\"", RegexOptions.IgnoreCase);
            Console.WriteLine(message);

            message = Regex.Replace("<secret>0254</secret><pin>0254</pin>", "<(secret)>(.*?)</secret>", "<$1>***</$1>", RegexOptions.IgnoreCase);
            Console.WriteLine(message);


            Assert.IsTrue(message.Contains("***"));
        }

        [Test]
        public async Task Will_mextract_xml_content()
        {
            var response = "<root><quotes>\r\n        <quote>\r\n            <quoteid>1422742</quoteid>\r\n            <feefri>FRI:233550000092/MSISDN</feefri>\r\n            <fee>\r\n                <amount>0.00</amount>\r\n                <currency>GHS</currency>\r\n            </fee>\r\n            <taxdetails>\r\n                <taxdetail>\r\n                    <name>CUSTOM_MOBILEMONEYTAX</name>\r\n                    <amount>\r\n                        <amount>0.01</amount>\r\n                        <currency>GHS</currency>\r\n                    </amount>\r\n                </taxdetail>\r\n            </taxdetails>\r\n            <accountbalance>\r\n                <amount>188.81</amount>\r\n                <currency>GHS</currency>\r\n            </accountbalance>\r\n            <receiveamount>\r\n                <amount>1.00</amount>\r\n                <currency>GHS</currency>\r\n            </receiveamount>\r\n            <sendamount>\r\n                <amount>1.00</amount>\r\n                <currency>GHS</currency>\r\n            </sendamount>\r\n        </quote>\r\n    </quotes></root>";
            var xml = new XmlDocument();
            xml.LoadXml(response);

            var Amount = Convert.ToDouble(xml.SelectSingleNode("//amount[1]/amount[1]")?.InnerText);
            var Currency = xml.SelectSingleNode("//amount[1]/currency[1]")?.InnerText;
            var Name = xml.SelectSingleNode("//name[1]")?.InnerText;


            Assert.IsTrue(response.Contains("***"));
        }

        [Test]
        public async Task Will_Shuffule()
        {
            var shuffled = Utility.ShuffleString("14d96a9b-904b-4cf8-963d-4725ead6f3b4".ToUpper().Split("-"));

            Assert.IsTrue(true);
        }

        [Test]
        public async Task Will_Get_value_between_characters()
        {
            string jsonString = "{\"name\": \"@sum($.numbers.S1)\", \"age\": \"($.age)\", \"phone\": \"@test($.number)\", \"join\": \"@join(\"_\", $.list[0], $.list[1], \"special\")\"}";
            dynamic data = new
            {
                numbers = new { S1 = 10, S2 = 20, S3 = 30 },
                age = 25,
                number = "233554081875",
                list = new string[] {"pink", "green has spaces"}
            };



            // Replace JSONPath expressions with evaluated values
            var template = new JsonEngine();
            template
                .RegisterClass(typeof(Utility))
                .RegisterFunction("test", (args) => 1 + 1);

            var newJson = template.Transform(jsonString, data);

            Assert.IsTrue(newJson == "{\"name\": \"10\", \"age\": 25, \"phone\": \"2\"}");
        }

    }
}

