using System;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EntryAttribute : Attribute
    {
        public string Route { get; set; }
        public string Method { get; set; }
        public EntryAttribute(string method = "POST", string route = null)
        {
            Route = route;
            Method = method;
        }
    }
}
