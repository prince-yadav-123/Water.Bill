using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Water.Bill.Infrastructure.Security;

public sealed class FileUploadSecurityOptions
{
    public long MaxBytes { get; init; }
    public IReadOnlySet<string> AllowedExtensions { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

public static class FileUploadSecurityHelper
{
    private static readonly IReadOnlyDictionary<string, string[]> MimeTypesByExtension =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = ["application/pdf", "application/x-pdf"],
            [".jpg"] = ["image/jpeg", "image/pjpeg"],
            [".jpeg"] = ["image/jpeg", "image/pjpeg"],
            [".png"] = ["image/png", "image/x-png"]
        };

    public static FileUploadSecurityOptions BuildOptions(IConfiguration configuration)
    {
        var maxBytes = (configuration.GetValue<int?>("FileStorage:MaxUploadSizeMb") ?? 5) * 1024L * 1024L;
        var extensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>() ?? [".pdf", ".jpg", ".jpeg", ".png"];

        return new FileUploadSecurityOptions
        {
            MaxBytes = maxBytes,
            AllowedExtensions = new HashSet<string>(
                extensions
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.StartsWith('.') ? x.Trim() : $".{x.Trim()}"),
                StringComparer.OrdinalIgnoreCase)
        };
    }

    public static bool TryValidate(IFormFile file, FileUploadSecurityOptions options, out string? errorMessage)
    {
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (file.Length <= 0)
        {
            errorMessage = "Uploaded file is empty.";
            return false;
        }

        if (file.Length > options.MaxBytes)
        {
            errorMessage = $"File {file.FileName} exceeds allowed upload size.";
            return false;
        }

        if (!options.AllowedExtensions.Contains(extension))
        {
            errorMessage = $"File type {extension} is not allowed.";
            return false;
        }

        if (!MimeTypesByExtension.TryGetValue(extension, out var allowedMimeTypes) ||
            !allowedMimeTypes.Contains(file.ContentType ?? string.Empty, StringComparer.OrdinalIgnoreCase))
        {
            errorMessage = $"Content type for {file.FileName} is not allowed.";
            return false;
        }

        errorMessage = null;
        return true;
    }

    public static string ResolveSafeContentType(string? fileNameOrPath)
    {
        var extension = Path.GetExtension(fileNameOrPath ?? string.Empty)?.ToLowerInvariant() ?? string.Empty;
        return MimeTypesByExtension.TryGetValue(extension, out var mimeTypes)
            ? mimeTypes[0]
            : "application/octet-stream";
    }

    public static bool TryResolveSafeStoredFilePath(string basePath, string? relativePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(relativePath))
            return false;

        var normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var rootFullPath = Path.GetFullPath(basePath);
        var combinedFullPath = Path.GetFullPath(Path.Combine(rootFullPath, normalizedRelativePath));

        var rootPrefix = rootFullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!combinedFullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(combinedFullPath, rootFullPath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = combinedFullPath;
        return true;
    }
}
