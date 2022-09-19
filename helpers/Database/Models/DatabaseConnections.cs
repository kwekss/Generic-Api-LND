using System.Collections.Generic;

namespace helpers.Database.Models
{
    public class Connection
    {
        public string Name { get; set; }
        public string Schema { get; set; }
        public string ConnectionString { get; set; }
        public List<Extra> Extra { get; set; }
    }

    public class Extra
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
