using System;

namespace models
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ApiDocAttribute : Attribute
    {
        public string Description { get; set; }
        public object Default { get; set; }
        public ApiDocAttribute()
        {

        }
    }
}
