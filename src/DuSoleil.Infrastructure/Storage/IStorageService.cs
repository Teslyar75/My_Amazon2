using Microsoft.AspNetCore.Http;

namespace DuSoleil.Infrastructure.Storage;

/// <summary>Сохранение загруженных изображений на диск (из homework DiskStorageService).</summary>
public interface IStorageService
{
    string Save(IFormFile formFile);
    byte[] Load(string filename);
}
