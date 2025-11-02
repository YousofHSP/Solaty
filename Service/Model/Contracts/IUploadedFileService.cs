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
        Task SetDisableFilesAsync(CancellationToken ct, string modelType, long modelId, UploadedFileType? type);
        Task<string> UploadFileAsync(IFormFile file, UploadedFileType type, string modelType, long modelId, long userId, CancellationToken ct);
        Task<string> GetFilePath(string modelType, long modelId, UploadedFileType type, CancellationToken ct);
        string GetFilePath(UploadedFile model, CancellationToken ct);
        Task<List<UploadedFile>> GetFiles(string modelType, List<long> modelIds, UploadedFileType? type, CancellationToken ct);
    }
}
