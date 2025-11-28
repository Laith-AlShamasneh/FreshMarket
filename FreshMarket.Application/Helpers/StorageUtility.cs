using FreshMarket.Shared.Helpers;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace FreshMarket.Application.Helpers;

/// <summary>
/// Utility methods for saving, retrieving and deleting files on disk.
/// Designed to be used from a shared project (no ASP.NET pipeline types returned).
/// </summary>
internal static class StorageUtilityHelper
{
    private const int LargeFileBufferSize = 1 * 1024 * 1024; // 1 MB
    private const int SmallFileBufferSize = 80 * 1024;       // 80 KB
    private const long LargeFileThreshold = 10L * 1024 * 1024; // 10 MB

    private static readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    internal enum FolderPathName
    {
        UsersImages
    }

    internal static Dictionary<FolderPathName, string> FolderPathNameDictionary = new()
    {
        { FolderPathName.UsersImages , "assets\\users\\images" }
    };

    #region Save

    /// <summary>
    /// Save an uploaded file to disk. Returns metadata about saved file.
    /// </summary>
    /// <returns>FileSaveResult containing locations and metadata. Throws exception on failure.</returns>
    internal static async Task<FileSaveResult> SaveFileAsync(
        string rootPath,
        string? relativeFolder,
        IFormFile formFile,
        FileUploadType fileType,
        string? customFileName = null,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(formFile, nameof(formFile));
        Guard.AgainstNull(rootPath, nameof(rootPath));

        string[] allowedExtensions = GetAllowedExtensions(fileType);

        var extension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
        if (allowedExtensions is not null && allowedExtensions.Length > 0)
        {
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"File extension '{extension}' is not allowed.");
        }

        var normalizedFolder = NormalizeRelativeFolder(relativeFolder);
        var folderName = string.IsNullOrWhiteSpace(normalizedFolder) ? string.Empty : normalizedFolder.Trim('/');
        var targetFolderName = Path.Combine(folderName, Guid.NewGuid().ToString());
        var fullDirectoryPath = Path.Combine(rootPath, targetFolderName);

        EnsureDirectoryExists(fullDirectoryPath);

        string sanitizedBaseName;
        if (!string.IsNullOrWhiteSpace(customFileName))
            sanitizedBaseName = SanitizeFileName(customFileName!);
        else
            sanitizedBaseName = Guid.NewGuid().ToString("N");

        var finalFileName = sanitizedBaseName + extension;
        var fullFilePath = Path.Combine(fullDirectoryPath, finalFileName);

        int bufferSize = formFile.Length > LargeFileThreshold ? LargeFileBufferSize : SmallFileBufferSize;

        await using (var writeStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await using var readStream = formFile.OpenReadStream();

            await readStream.CopyToAsync(writeStream, bufferSize, cancellationToken).ConfigureAwait(false);
        }

        if (!_contentTypeProvider.TryGetContentType(finalFileName, out var contentType))
            contentType = "application/octet-stream";

        var relativePath = CombineRelativePathParts(folderName, Path.GetFileName(targetFolderName), finalFileName);

        return new FileSaveResult(
            FolderName: Path.GetFileName(targetFolderName),
            FileName: finalFileName,
            RelativePath: relativePath,
            FullPath: fullFilePath,
            Extension: extension,
            ContentType: contentType,
            Size: formFile.Length);
    }

    #endregion

    #region Get

    /// <summary>
    /// Open a file stream given a full or relative path. If relative path is supplied it's combined with rootPath.
    /// Returns FileGetResult or null if not found.
    /// Caller must dispose the returned Stream.
    /// </summary>
    internal static async Task<FileGetWithUrlResult?> GetFileAsync(
    string rootPath,
    string pathOrRelativeFolder,
    string fileNameWithExtension,
    string? baseUrl = null,
    CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(rootPath, nameof(rootPath));
        Guard.AgainstNull(fileNameWithExtension, nameof(fileNameWithExtension));

        cancellationToken.ThrowIfCancellationRequested();

        var trimmedFolder = string.IsNullOrWhiteSpace(pathOrRelativeFolder) ? string.Empty : pathOrRelativeFolder.Trim();
        var trimmedFileName = fileNameWithExtension.Trim();

        string candidateFullPath;
        if (Path.IsPathRooted(trimmedFileName))
        {
            candidateFullPath = trimmedFileName;
        }
        else
        {
            string baseFolder;
            if (!string.IsNullOrWhiteSpace(trimmedFolder) && Path.IsPathRooted(trimmedFolder))
                baseFolder = trimmedFolder;
            else if (!string.IsNullOrWhiteSpace(trimmedFolder))
                baseFolder = Path.Combine(rootPath, trimmedFolder.TrimStart('/', '\\'));
            else
                baseFolder = rootPath;

            if (!string.IsNullOrWhiteSpace(trimmedFolder) &&
                (trimmedFolder.EndsWith(trimmedFileName, StringComparison.OrdinalIgnoreCase) ||
                 Path.GetFileName(trimmedFolder).Equals(trimmedFileName, StringComparison.OrdinalIgnoreCase)))
            {
                candidateFullPath = Path.Combine(rootPath, trimmedFolder.TrimStart('/', '\\'));
            }
            else
            {
                candidateFullPath = Path.Combine(baseFolder, trimmedFileName);
            }
        }

        try
        {
            candidateFullPath = Path.GetFullPath(candidateFullPath);
        }
        catch
        {
            return null;
        }

        if (!File.Exists(candidateFullPath))
            return null;

        var fileInfo = new FileInfo(candidateFullPath);

        var stream = new FileStream(candidateFullPath, FileMode.Open, FileAccess.Read, FileShare.Read, SmallFileBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

        if (!_contentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType))
            contentType = "application/octet-stream";

        string? url = null;
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            try
            {
                var rootFull = Path.GetFullPath(rootPath);
                if (candidateFullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                {
                    var relative = Path.GetRelativePath(rootFull, candidateFullPath).Replace('\\', '/');
                    url = $"{baseUrl.TrimEnd('/')}/{relative.TrimStart('/')}";
                }
                else
                {
                    url = $"{baseUrl.TrimEnd('/')}/{fileInfo.Name}";
                }
            }
            catch
            {
                url = null;
            }
        }

        return await Task.FromResult(new FileGetWithUrlResult(
            FileName: fileInfo.Name,
            FullPath: candidateFullPath,
            Stream: stream,
            ContentType: contentType,
            Size: fileInfo.Length,
            Extension: fileInfo.Extension,
            Url: url));
    }

    /// <summary>
    /// Search a folder for a file by name without extension and return the first match (case-insensitive).
    /// Useful when DB only stores a name/identifier and extension is saved in DB or not known.
    /// </summary>
    internal static async Task<FileGetWithUrlResult?> GetFileByNameWithoutExtensionAsync(
    string rootPath,
    string relativeFolderPath,
    string fileNameWithoutExtension,
    string? baseUrl = null,
    CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(rootPath, nameof(rootPath));
        Guard.AgainstNull(relativeFolderPath, nameof(relativeFolderPath));
        Guard.AgainstNull(fileNameWithoutExtension, nameof(fileNameWithoutExtension));
        cancellationToken.ThrowIfCancellationRequested();

        var fullFolderPath = Path.Combine(rootPath, relativeFolderPath.TrimStart('/', '\\'));

        if (!Directory.Exists(fullFolderPath))
            return null;

        var sanitized = fileNameWithoutExtension.Trim();

        var matched = Directory.GetFiles(fullFolderPath)
            .FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f).Trim(), sanitized, StringComparison.OrdinalIgnoreCase));

        if (matched == null)
            return null;

        var fileNameWithExtension = Path.GetFileName(matched);
        return await GetFileAsync(rootPath, relativeFolderPath, fileNameWithExtension, baseUrl, cancellationToken);
    }

    /// <summary>
    /// Returns metadata-only list of file URLs or file metadata for all files inside a relative folder.
    /// If baseUrl is provided, Url property on the result will be populated.
    /// Caller can then call GetFileAsync to open streams as needed.
    /// </summary>
    internal static Task<IList<FileInfoResult>> GetFilesFromFolderAsync(string rootPath, string relativeFolderPath, string? baseUrl = null, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(rootPath, nameof(rootPath));
        Guard.AgainstNull(relativeFolderPath, nameof(relativeFolderPath));

        var fullFolderPath = Path.Combine(rootPath, relativeFolderPath.TrimStart('/', '\\'));

        if (!Directory.Exists(fullFolderPath))
            return Task.FromResult((IList<FileInfoResult>)Array.Empty<FileInfoResult>());

        var files = Directory.GetFiles(fullFolderPath)
            .Where(f => (File.GetAttributes(f) & FileAttributes.Directory) == 0)
            .Select(f =>
            {
                var fi = new FileInfo(f);
                var rel = CombineRelativePathParts(relativeFolderPath.Trim('/'), fi.Name);
                var url = !string.IsNullOrWhiteSpace(baseUrl) ? $"{baseUrl!.TrimEnd('/')}/{rel.Replace('\\', '/')}" : null;
                return new FileInfoResult(fi.Name, rel, fi.FullName, fi.Length, fi.Extension, url);
            })
            .ToList();

        return Task.FromResult((IList<FileInfoResult>)files);
    }

    #endregion

    #region Delete

    internal static Task<bool> DeleteFileAsync(string fullFilePath)
    {
        Guard.AgainstNull(fullFilePath, nameof(fullFilePath));

        if (!File.Exists(fullFilePath))
            return Task.FromResult(false);

        File.Delete(fullFilePath);
        return Task.FromResult(true);
    }

    internal static Task<bool> DeleteFolderAsync(string rootPath, string relativeFolderPath)
    {
        Guard.AgainstNull(rootPath, nameof(rootPath));
        Guard.AgainstNull(relativeFolderPath, nameof(relativeFolderPath));

        var fullPath = Path.Combine(rootPath, relativeFolderPath.TrimStart('/', '\\'));

        if (!Directory.Exists(fullPath))
            return Task.FromResult(false);

        Directory.Delete(fullPath, recursive: true);
        return Task.FromResult(true);
    }

    #endregion

    #region Helpers

    private static void EnsureDirectoryExists(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private static string NormalizeRelativeFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return string.Empty;
        var normalized = folder.Replace('\\', '/').Trim('/');
        return normalized;
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Where(c => !invalid.Contains(c)));
    }

    private static string CombineRelativePathParts(params string[] parts)
    {
        var filtered = parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim('/', '\\'));
        return string.Join('/', filtered);
    }

    private static string[] GetAllowedExtensions(FileUploadType type) => type switch
    {
        FileUploadType.UserProfileImage => [".png", ".jpg", ".jpeg"],
        _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown file upload type: {type}")
    };

    #endregion

    #region DTOs / Results

    /// <summary>
    /// Result returned after saving a file.
    /// </summary>
    internal sealed record FileSaveResult(
        string FolderName,
        string FileName,
        string RelativePath,
        string FullPath,
        string Extension,
        string ContentType,
        long Size);

    /// <summary>
    /// Result returned when opening a file for reading with URL. Caller must dispose Stream.
    /// </summary>
    internal sealed record FileGetWithUrlResult(
        string FileName,
        string FullPath,
        Stream Stream,
        string ContentType,
        long Size,
        string Extension,
        string? Url);

    /// <summary>
    /// Lightweight metadata about files inside a folder (no Stream).
    /// </summary>
    internal sealed record FileInfoResult(
        string FileName,
        string RelativePath,
        string FullPath,
        long Size,
        string Extension,
        string? Url);

    #endregion
}

/// <summary>
/// Lightweight replacement for Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider.
/// Provides a mapping of file extensions to MIME/content types and a TryGetContentType API.
/// Thread-safe and extensible at runtime via AddMapping.
/// </summary>
internal sealed class FileExtensionContentTypeProvider
{
    private readonly ConcurrentDictionary<string, string> _map;

    internal FileExtensionContentTypeProvider()
    {
        _map = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        InitializeDefaults();
    }

    internal bool TryGetContentType(string fileNameOrExtension, out string contentType)
    {
        contentType = null!;

        if (string.IsNullOrWhiteSpace(fileNameOrExtension))
            return false;

        string ext = Path.HasExtension(fileNameOrExtension)
            ? Path.GetExtension(fileNameOrExtension)
            : fileNameOrExtension;

        if (string.IsNullOrEmpty(ext))
            return false;

        if (!ext.StartsWith('.'))
            ext = '.' + ext;

        return _map.TryGetValue(ext, out contentType!);
    }

    /// <summary>
    /// Add or update a mapping from extension (with or without leading dot) to content type.
    /// Returns true if added or updated.
    /// </summary>
    internal bool AddMapping(string extension, string contentType)
    {
        Guard.AgainstNull(extension, nameof(extension));
        Guard.AgainstNull(contentType, nameof(contentType));

        var ext = extension.StartsWith('.') ? extension : '.' + extension;
        _map[ext] = contentType;
        return true;
    }

    /// <summary>
    /// Remove a mapping for the specified extension (with or without leading dot).
    /// Returns true if removed.
    /// </summary>
    internal bool RemoveMapping(string extension)
    {
        Guard.AgainstNull(extension, nameof(extension));
        var ext = extension.StartsWith('.') ? extension : '.' + extension;
        return _map.TryRemove(ext, out _);
    }

    /// <summary>
    /// Returns a snapshot of current mappings (read-only).
    /// </summary>
    internal IReadOnlyDictionary<string, string> GetMappings() => new Dictionary<string, string>(_map, StringComparer.OrdinalIgnoreCase);

    private void InitializeDefaults()
    {
        // Common types
        _map[".txt"] = "text/plain";
        _map[".csv"] = "text/csv";
        _map[".htm"] = "text/html";
        _map[".html"] = "text/html";
        _map[".css"] = "text/css";
        _map[".js"] = "application/javascript";
        _map[".json"] = "application/json";
        _map[".xml"] = "application/xml";
        _map[".pdf"] = "application/pdf";
        _map[".zip"] = "application/zip";
        _map[".tar"] = "application/x-tar";
        _map[".gz"] = "application/gzip";
        _map[".rar"] = "application/vnd.rar";
        _map[".7z"] = "application/x-7z-compressed";
        _map[".rtf"] = "application/rtf";
        _map[".eml"] = "message/rfc822";

        // Office / documents
        _map[".doc"] = "application/msword";
        _map[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        _map[".xls"] = "application/vnd.ms-excel";
        _map[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        _map[".ppt"] = "application/vnd.ms-powerpoint";
        _map[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation";

        // Images
        _map[".png"] = "image/png";
        _map[".jpg"] = "image/jpeg";
        _map[".jpeg"] = "image/jpeg";
        _map[".gif"] = "image/gif";
        _map[".bmp"] = "image/bmp";
        _map[".webp"] = "image/webp";
        _map[".svg"] = "image/svg+xml";
        _map[".ico"] = "image/x-icon";

        // Audio / video
        _map[".mp3"] = "audio/mpeg";
        _map[".wav"] = "audio/wav";
        _map[".ogg"] = "audio/ogg";
        _map[".mp4"] = "video/mp4";
        _map[".mov"] = "video/quicktime";
        _map[".avi"] = "video/x-msvideo";
        _map[".webm"] = "video/webm";
        _map[".mpeg"] = "video/mpeg";
        _map[".m4a"] = "audio/mp4";

        // Excel / spreadsheet alternatives
        _map[".ods"] = "application/vnd.oasis.opendocument.spreadsheet";
        _map[".odp"] = "application/vnd.oasis.opendocument.presentation";

    }
}
