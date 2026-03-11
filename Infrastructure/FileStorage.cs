using System.Security.Cryptography;

namespace gateway.Infrastructure;

public sealed class FileStorage
{
    private readonly IWebHostEnvironment _environment;

    public FileStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IReadOnlyList<string>> SaveUploadsAsync(IEnumerable<IFormFile>? files, CancellationToken cancellationToken)
    {
        if (files is null)
        {
            return [];
        }

        var uploadRoot = Path.Combine(_environment.ContentRootPath, "uploads", "items");
        Directory.CreateDirectory(uploadRoot);

        var saved = new List<string>();
        foreach (var file in files)
        {
            if (file.Length <= 0)
            {
                continue;
            }

            var extension = Path.GetExtension(file.FileName);
            await using var input = file.OpenReadStream();
            using var memory = new MemoryStream();
            await input.CopyToAsync(memory, cancellationToken);

            var bytes = memory.ToArray();
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            var safeName = $"{hash}{extension}";
            var destination = Path.Combine(uploadRoot, safeName);

            if (!File.Exists(destination))
            {
                await File.WriteAllBytesAsync(destination, bytes, cancellationToken);
            }

            saved.Add($"uploads/items/{safeName}");
        }

        return saved;
    }
}
