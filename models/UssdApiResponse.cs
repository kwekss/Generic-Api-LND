using System.Collections.Generic;

namespace models
{
    public class UssdApiResponse
    {
        public bool Success { get; set; }
        public string ResponseBody { get; set; }
        public string Header { get; set; }
        public int MenuItemId { get; set; }
        public List<OptionItem> options { get; set; }
    }

    public class OptionItem
    {
        public string optionItem { get; set; }
        public bool overideDisplay { get; set; }
        public string optionValue { get; set; }
        public long optionCategoryId { get; set; }
    }
}
