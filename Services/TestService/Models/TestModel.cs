using models;

namespace TestService.Models
{
    public class TestModel
    {
        public string Prop { get; set; }
        [ApiDoc(Description = "Some description for data object", Default = 1234)]
        public int Id { get; set; }
        
        public DataModel Data { get; set; }
    }
    public class DataModel
    {
        public string Prop { get; set; }
        public DataSubModel? data { get; set; }
    }
    public class DataSubModel
    {
        public string Prop { get; set; }
    }
}
