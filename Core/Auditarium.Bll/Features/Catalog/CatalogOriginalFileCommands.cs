// SPDX-License-Identifier: MIT
using Auditarium.Bll.Abstractions.Files;
using Auditarium.Bll.Abstractions.Identity;
using Auditarium.Bll.Abstractions.Persistence;
using Auditarium.Bll.Abstractions.Settings;
using Auditarium.Bll.Security;
using Auditarium.Common.Results;
using Auditarium.Models.Catalog;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Auditarium.Bll.Features.Catalog;

public sealed record CatalogOriginalFile(string OriginalFileName, string ContentType, long Size, Stream Content);
[RequiresPermission("Documents.Manage")]
public sealed record ReplaceCatalogOriginalFileCommand(long CatalogVersionId, string OriginalFileName, string ContentType, Stream Content) : IRequest<Result>;
[RequiresPermission("Documents.Manage")]
public sealed record RemoveCatalogOriginalFileCommand(long CatalogVersionId) : IRequest<Result>;
[RequiresPermission("Documents.Manage")]
public sealed record GetCatalogOriginalFileQuery(long CatalogVersionId) : IRequest<Result<CatalogOriginalFile>>;

public sealed class CatalogOriginalFileHandler(IAuditariumDbContext db, IFileStorage storage, IApplicationSettingResolver settings, ICurrentActor actor) :
    IRequestHandler<ReplaceCatalogOriginalFileCommand, Result>, IRequestHandler<RemoveCatalogOriginalFileCommand, Result>, IRequestHandler<GetCatalogOriginalFileQuery, Result<CatalogOriginalFile>>
{
    public async ValueTask<Result> Handle(ReplaceCatalogOriginalFileCommand message, CancellationToken ct)
    {
        if (!IsPdfMetadata(message.OriginalFileName, message.ContentType) || !message.Content.CanRead) return Failure("CATALOG.ORIGINAL_FILE_INVALID", ErrorType.Validation);
        var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == message.CatalogVersionId, ct);
        if (catalog is null) return Failure("CATALOG.NOT_FOUND", ErrorType.NotFound);
        if (catalog.CatalogState != CatalogState.Draft) return Failure("CATALOG.NOT_DRAFT", ErrorType.Conflict);
        byte[] prefix;
        try { prefix = await ReadPrefixAsync(message.Content, ct); }
        catch (InvalidDataException) { return Failure("CATALOG.ORIGINAL_FILE_INVALID", ErrorType.Validation); }
        if (!prefix.AsSpan().SequenceEqual("%PDF-"u8)) return Failure("CATALOG.ORIGINAL_FILE_INVALID", ErrorType.Validation);
        var maximumSize = await LimitAsync("Files:OriginalDocuments:MaxUploadSize", ct); StoredFile stored;
        try { stored = await storage.StoreAsync(new PrefixStream(prefix, message.Content), maximumSize, ct); }
        catch (FileTooLargeException) { return Failure("CATALOG.ORIGINAL_FILE_TOO_LARGE", ErrorType.Validation); }
        var oldFileId = catalog.SourceFileId;
        try
        {
            var file = new FileItem { OriginalFileName = Path.GetFileName(message.OriginalFileName), SaveFileName = stored.SaveFileName, Extension = ".pdf", SaveFilePath = stored.SaveFilePath, ContentType = "application/pdf", Size = stored.Size, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = actor.UserId ?? 0, Checksum = stored.Checksum };
            db.FileItems.Add(file); await db.SaveChangesAsync(ct); catalog.SourceFileId = file.FileId; catalog.DraftRevision++; await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(stored.SaveFilePath, stored.SaveFileName, ct);
            throw;
        }
        if (oldFileId is { } old) await CleanupUnreferencedFileAsync(old, ct);
        return Result.Success();
    }

    public async ValueTask<Result> Handle(RemoveCatalogOriginalFileCommand message, CancellationToken ct)
    {
        var catalog = await db.CatalogVersions.SingleOrDefaultAsync(x => x.CatalogVersionId == message.CatalogVersionId, ct);
        if (catalog is null) return Failure("CATALOG.NOT_FOUND", ErrorType.NotFound);
        if (catalog.CatalogState != CatalogState.Draft) return Failure("CATALOG.NOT_DRAFT", ErrorType.Conflict);
        var oldFileId = catalog.SourceFileId; if (oldFileId is null) return Result.Success();
        catalog.SourceFileId = null; catalog.DraftRevision++; await db.SaveChangesAsync(ct); await CleanupUnreferencedFileAsync(oldFileId.Value, ct); return Result.Success();
    }

    public async ValueTask<Result<CatalogOriginalFile>> Handle(GetCatalogOriginalFileQuery message, CancellationToken ct)
    {
        var file = await (from catalog in db.CatalogVersions where catalog.CatalogVersionId == message.CatalogVersionId && catalog.SourceFileId != null join item in db.FileItems on catalog.SourceFileId equals item.FileId select item).SingleOrDefaultAsync(ct);
        if (file is null) return Failure<CatalogOriginalFile>("CATALOG.ORIGINAL_FILE_NOT_FOUND", ErrorType.NotFound);
        if (file.Size > await LimitAsync("Files:OriginalDocuments:MaxDownloadSize", ct)) return Failure<CatalogOriginalFile>("CATALOG.ORIGINAL_FILE_TOO_LARGE", ErrorType.Validation);
        if (!await storage.ExistsAsync(file.SaveFilePath, file.SaveFileName, ct)) return Failure<CatalogOriginalFile>("CATALOG.ORIGINAL_FILE_INTEGRITY_ERROR", ErrorType.Failure);
        return Result<CatalogOriginalFile>.Success(new(file.OriginalFileName, file.ContentType, file.Size, await storage.OpenReadAsync(file.SaveFilePath, file.SaveFileName, ct)));
    }

    private async Task CleanupUnreferencedFileAsync(long fileId, CancellationToken ct)
    {
        if (await db.CatalogVersions.AnyAsync(x => x.SourceFileId == fileId, ct)) return;
        var file = await db.FileItems.SingleOrDefaultAsync(x => x.FileId == fileId, ct); if (file is null) return;
        await storage.DeleteAsync(file.SaveFilePath, file.SaveFileName, ct); db.FileItems.Remove(file); await db.SaveChangesAsync(ct);
    }
    private async Task<long> LimitAsync(string key, CancellationToken ct) => long.Parse((await settings.GetAsync(key, ct)).Value, global::System.Globalization.CultureInfo.InvariantCulture);
    private static bool IsPdfMetadata(string name, string type) => string.Equals(Path.GetExtension(name), ".pdf", StringComparison.OrdinalIgnoreCase) && string.Equals(type, "application/pdf", StringComparison.OrdinalIgnoreCase);
    private static async Task<byte[]> ReadPrefixAsync(Stream content, CancellationToken ct) { var prefix = new byte[5]; var offset = 0; while (offset < prefix.Length) { var read = await content.ReadAsync(prefix.AsMemory(offset), ct); if (read == 0) throw new InvalidDataException("File is too short."); offset += read; } return prefix; }
    private static Result Failure(string code, ErrorType type) => Result.Failure(new AppError(code, type));
    private static Result<T> Failure<T>(string code, ErrorType type) => Result<T>.Failure(new AppError(code, type));

    private sealed class PrefixStream(byte[] prefix, Stream inner) : Stream
    {
        private int _offset;
        public override bool CanRead => true; public override bool CanSeek => false; public override bool CanWrite => false; public override long Length => throw new NotSupportedException(); public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default) { if (_offset < prefix.Length) { var count = Math.Min(buffer.Length, prefix.Length - _offset); prefix.AsMemory(_offset, count).CopyTo(buffer); _offset += count; return count; } return await inner.ReadAsync(buffer, ct); }
        public override void Flush() => throw new NotSupportedException(); public override Task FlushAsync(CancellationToken ct) => throw new NotSupportedException(); public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException(); public override void SetLength(long value) => throw new NotSupportedException(); public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
