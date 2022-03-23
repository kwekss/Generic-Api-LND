using System;
using System.Reflection;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FeatureAttribute : Attribute
    {
        public string Name { get; set; }
        public void Init(object instance, MethodBase method, object[] args)
        {
            Console.WriteLine(string.Format("Init: {0} [{1}]", method.DeclaringType.FullName + "." + method.Name, args.Length));
        }

        public void OnEntry()
        {
            Console.WriteLine("OnEntry");
        }

        public void OnExit()
        {
            Console.WriteLine("OnExit");
        }

        public void OnException(Exception exception)
        {
            Console.WriteLine(string.Format("OnException: {0}: {1}", exception.GetType(), exception.Message));
        }
    }
}
