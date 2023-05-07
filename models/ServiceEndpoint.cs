using Newtonsoft.Json;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace models
{
    public enum AuthenticationType
    {
        Integrator, InternalBearerToken, OpenIdToken
    }
    public class ServiceEndpoint
    {
        public ServiceEndpoint(List<string> path)
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
        public byte[] RequestBody { get; set; }
        public dynamic RequestBodyToObject()
        { 
            var data = Encoding.UTF8.GetString(RequestBody);
            return JsonConvert.DeserializeObject(data);
        }
        public List<FileContent> Files { get; set; }
        public ClaimsPrincipal User { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
    }

    public class FileContent
    {
        public byte[] Content { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
    }
}
