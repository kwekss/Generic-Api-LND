using System.Collections.Generic;

namespace models
{
    public class Endpoint
    {
        public Endpoint(List<string> path)
        {
            _path = path;
            Service = path[0];
            Feature = path[1];
            if (path.Count > 2)
            {
                path.RemoveRange(0, 2);
                Route = string.Join('/', path);
            }
                
        }
        public string Service { get; }
        public string Feature { get; }
        public string Route { get; }
        public string RequiredFullname { get => $"{Service}Service.Features"; }
        private List<string> _path { get; }
    }
}
