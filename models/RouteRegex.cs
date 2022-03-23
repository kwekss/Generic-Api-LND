using System.Collections.Generic;

namespace models
{
    public class RouteRegex
    {
        public string Regex { get; set; }
        public List<RouteRegexParam> RouteParams { get; set; }
    }
    public class RouteRegexParam
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }
        public string Value { get; set; }
    }
}
