// SPDX-License-Identifier: MIT
namespace Auditarium.Bll.Abstractions.Files;

public sealed record StoredFile(string SaveFileName, string SaveFilePath, long Size, string Checksum);
public sealed class FileTooLargeException : Exception { public FileTooLargeException() : base("File exceeds configured maximum size.") { } }
public interface IFileStorage
{
    Task<StoredFile> StoreAsync(Stream content, long maximumSize, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string saveFilePath, string saveFileName, CancellationToken cancellationToken = default);
}
