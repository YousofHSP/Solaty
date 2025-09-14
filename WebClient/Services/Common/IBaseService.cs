namespace WebClient.Services.Common;

public interface IBaseService
{
    Task<TResDto?> Post<TDto, TResDto>(string uri, TDto dto) where TResDto : class, new();
    Task<bool> Post<TDto>(string uri, TDto dto);
    Task<bool> Post(string uri);
    Task<TResDto?> Patch<TDto, TResDto>(string uri, TDto dto) where TResDto : class, new();
    Task<TResDto?> Get<TResDto>(string uri) where TResDto : class;
    Task<bool> Get(string uri);
    Task<byte[]?> GetFile(string uri);
    Task<bool> Delete(string uri);
    HttpClient GetHttpClient();
}