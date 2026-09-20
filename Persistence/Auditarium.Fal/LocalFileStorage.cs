// SPDX-License-Identifier: MIT
using System.Security.Cryptography;
using Auditarium.Bll.Abstractions.Files;

namespace Auditarium.Fal;

/// <summary>Filesystem storage with opaque, collision-safe physical names.</summary>
public sealed class LocalFileStorage(string storageRoot) : IFileStorage
{
    private readonly string _storageRoot = Path.GetFullPath(storageRoot);

    public async Task<StoredFile> StoreAsync(Stream content, long maximumSize, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (maximumSize < 1) throw new ArgumentOutOfRangeException(nameof(maximumSize));
        const string relativePath = "documents/originals";
        var directory = Path.Combine(_storageRoot, "documents", "originals");
        Directory.CreateDirectory(directory);
        while (true)
        {
            var name = Guid.NewGuid().ToString("N"); var path = Path.Combine(directory, name);
            try
            {
                await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256); var buffer = new byte[81920]; long size = 0; int read;
                while ((read = await content.ReadAsync(buffer.AsMemory(), cancellationToken)) != 0)
                {
                    if (size > maximumSize - read) throw new FileTooLargeException();
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken); hash.AppendData(buffer, 0, read); size += read;
                }
                return new StoredFile(name, relativePath, size, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
            }
            catch (IOException) when (File.Exists(path)) { }
            catch
            {
                File.Delete(path);
                throw;
            }
        }
    }

    public Task<Stream> OpenReadAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(new FileStream(Resolve(saveFilePath, saveFileName), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous));
    public Task DeleteAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) { File.Delete(Resolve(saveFilePath, saveFileName)); return Task.CompletedTask; }
    public Task<bool> ExistsAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default) => Task.FromResult(File.Exists(Resolve(saveFilePath, saveFileName)));
    private string Resolve(string relativePath, string name)
    {
        if (Path.IsPathRooted(relativePath) || name != Path.GetFileName(name)) throw new InvalidOperationException("Invalid storage path.");
        var path = Path.GetFullPath(Path.Combine(_storageRoot, relativePath, name));
        if (!path.StartsWith(_storageRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new InvalidOperationException("Storage path escapes root.");
        return path;
    }
}
