using models;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TestService.Models;

namespace TestService.Providers
{
    public class UploadProvider : IUploadProvider
    {
        public async Task<UploadedFile> Upload(FileContent file, UploadConfiguration config)
        {
            try
            {
                if (!Directory.Exists(config.UploadDir))
                    Directory.CreateDirectory(config.UploadDir);

                var ext = Path.GetExtension(file.FileName).TrimStart(Convert.ToChar("."));
                if (file != null && file.FileSize < 0)
                    return new UploadedFile { Success = false, Message = "Unable to upload file. Invalid file uploaded" };


                if (config.AllowedExtensions != null || config.AllowedExtensions.Any())
                {
                    config.AllowedExtensions.ForEach(i => i = i.ToLower());
                    if (!config.AllowedExtensions.Contains(ext.ToLower()))
                        return new UploadedFile { Success = false, Message = "File not allowed for upload. Please check and try again" };
                }

                if ((file.FileSize / 1048576.0) > config.AllowedSizeMb)
                    return new UploadedFile { Success = false, Message = $"File size not allowed for upload. Max file size of {config.AllowedSizeMb}MB allowed. Please check and try again" };


                if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(file.FileName)))
                    return new UploadedFile { Success = false, Message = "Unable to process file upload. File name cannot be null." };

                var newName = Guid.NewGuid().ToString();
                var fileName = Path.GetFileName(file.FileName);
                var newFilename = $"{newName}.{ext}";
                var path = Path.Combine(config.UploadDir, newFilename);

                var uploadedFile = new UploadedFile
                {
                    FileName = fileName,
                    NewFileName = newFilename,
                    FileUuid = newName,
                    Success = true
                };

                await File.WriteAllBytesAsync(path, file.Content);
                return uploadedFile;

            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return new UploadedFile { Success = false, Message = "An error occured while processing file. Please try again" };
            }
        }

        public async Task<UploadedFile> MoveUploadedFile(string filename, string newLocation, UploadConfiguration config)
        {
            var oldFilePath = Path.Combine(config.UploadDir, filename);
            var newFilePath = Path.Combine(newLocation, filename);
            var errorResponse = new UploadedFile
            {
                FileName = oldFilePath,
                NewFileName = filename,
                FileUuid = Path.GetFileNameWithoutExtension(filename),
                Success = false
            };

            try
            {
                if (File.Exists(oldFilePath))
                {
                    File.Move(oldFilePath, newFilePath);
                    return new UploadedFile
                    {
                        FileName = newFilePath,
                        NewFileName = filename,
                        FileUuid = Path.GetFileNameWithoutExtension(filename),
                        Success = true
                    };
                }

                return errorResponse;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return errorResponse;
            }

        }
    }
}
