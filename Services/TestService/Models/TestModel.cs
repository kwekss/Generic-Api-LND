namespace TestService.Models
{
    public class TestModel
    {
        public string Prop { get; set; } 
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
