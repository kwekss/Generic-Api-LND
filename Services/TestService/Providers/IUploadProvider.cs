using models;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Providers
{
    public interface IUploadProvider
    {
        Task<UploadedFile> MoveUploadedFile(string filename, string newLocation, UploadConfiguration config);
        Task<UploadedFile> Upload(FileContent file, UploadConfiguration config);
    }
}