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
            var safeName = $"{Guid.NewGuid():N}{extension}";
            var destination = Path.Combine(uploadRoot, safeName);
            await using var stream = File.Create(destination);
            await file.CopyToAsync(stream, cancellationToken);
            saved.Add($"uploads/items/{safeName}");
        }

        return saved;
    }
}
