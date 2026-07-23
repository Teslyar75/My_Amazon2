using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Perry.Infrastructure.Storage;

/// <summary>
/// Файлы кладутся в wwwroot/uploads/ с GUID-именем.
/// Путь резолвится относительно ContentRoot веб-приложения.
/// </summary>
public class DiskStorageService : IStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly string _storagePath;

    public DiskStorageService(IHostEnvironment env)
    {
        _storagePath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(_storagePath);
    }

    public byte[] Load(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Empty file name", nameof(filename));

        var path = Path.Combine(_storagePath, filename);
        if (!File.Exists(path))
            throw new FileNotFoundException();

        return File.ReadAllBytes(path);
    }

    public string Save(IFormFile formFile)
    {
        ArgumentNullException.ThrowIfNull(formFile);

        if (formFile.Length == 0)
            throw new ArgumentException("File is empty", nameof(formFile));

        if (formFile.Length > MaxFileSize)
            throw new ArgumentException("File too large (max 5MB)", nameof(formFile));

        var ext = Path.GetExtension(formFile.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            throw new ArgumentException($"Extension '{ext}' is not allowed", nameof(formFile));

        var saveName = Guid.NewGuid() + ext;
        var savePath = Path.Combine(_storagePath, saveName);

        using var stream = File.Create(savePath);
        formFile.CopyTo(stream);

        return saveName;
    }
}
