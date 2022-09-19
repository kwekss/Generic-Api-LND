using System;

namespace models
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ApiDocAttribute : Attribute
    {
        public string Description { get; set; }
        public dynamic Default { get; set; }
        public ApiDocAttribute()
        {

        }
    }
}
