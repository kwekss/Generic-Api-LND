using System.Threading.Tasks;

namespace helpers.Interfaces
{
    public interface IDocumentationBuilder
    {
        Task RenderOpenApiSpecs(string name = null);
        Task RenderView(string name = null);
    }
}