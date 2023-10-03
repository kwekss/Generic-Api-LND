using System.Linq;

namespace helpers.Engine.Json
{
    public class InternalFunctions
    {
        public InternalFunctions()
        {

        }

        public double sum(params dynamic[] args)
        {
            return args.Select(x => (double)x).Sum();
        }
        public bool equals(params dynamic[] args)
        {
            return args.All(x => x == args.First());
        }
        public string join(string separator, params dynamic[] args)
        {
            return string.Join(separator, args);
        }
        public long length(string a)
        {
            return a.Length;
        }
    }
}
