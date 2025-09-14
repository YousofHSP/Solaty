using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Service.Model.Contracts
{
    public interface IUploadedFileService
    {
        Task SetDisableFilesAsync(CancellationToken ct, string modelType, int modelId, UploadedFileType? type);
        Task<string> UploadFileAsync(IFormFile file, UploadedFileType type, string modelType, int modelId, int userId, CancellationToken ct);
        Task<string> GetFilePath(string modelType, int modelId, UploadedFileType type, CancellationToken ct);
        string GetFilePath(UploadedFile model, CancellationToken ct);
        Task<List<UploadedFile>> GetFiles(string modelType, List<int> modelIds, UploadedFileType? type, CancellationToken ct);
    }
}
