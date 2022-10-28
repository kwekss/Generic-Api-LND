using helpers;
using helpers.Atttibutes;
using Microsoft.Extensions.Configuration;
using models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestService.Models;
using TestService.Providers;

namespace TestService.Features
{
    [ApiDoc(Description = "This is a test endpoint for upload")]
    [Feature(Name = "UploadDocument")]
    public class UploadDocument : BaseServiceFeature
    {
        private readonly IUploadProvider _uploadProvider;
        private readonly string _baseUploadPath;
        private readonly List<string> _allowed_doc_types = new List<string>();
        private readonly UploadConfiguration _upload_doc_config;

        public UploadDocument(IUploadProvider uploadProvider, IConfiguration config) : base()
        {
            _uploadProvider = uploadProvider;
            _allowed_doc_types = config.GetValue("ALLOWED_DOC_TYPES", "pdf").Split(",").ToList();
            _baseUploadPath = config.GetValue("BASE_UPLOAD_DIR", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads"));

            if (string.IsNullOrWhiteSpace(_baseUploadPath))
                _baseUploadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");

            _upload_doc_config = new UploadConfiguration
            {

                AllowedExtensions = _allowed_doc_types,
                UploadDir = _baseUploadPath,
                AllowedSizeMb = config.GetValue("MAX_FILE_SIZE_MB", 4)
            };
        }

        [Entry(Method = "POST")]
        public async Task<ApiResponse> Entry([FromFormBody] FileContent image)
        {
            var uploadResponse = await _uploadProvider.Upload(image, _upload_doc_config);
            
            if (!uploadResponse.Success)
                return new ApiResponse { Success = false, ResponseMessage = string.IsNullOrWhiteSpace(uploadResponse.Message) ? "Upload failed. Please try again later" : uploadResponse.Message };
            
            return new ApiResponse { Success = true, Data = uploadResponse };
        }

    }
}
