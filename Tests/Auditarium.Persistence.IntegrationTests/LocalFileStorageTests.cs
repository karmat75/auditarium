// SPDX-License-Identifier: MIT
using System.Security.Cryptography;
using Auditarium.Bll.Abstractions.Files;
using Auditarium.Fal;
using Xunit;

namespace Auditarium.Persistence.IntegrationTests;

public sealed class LocalFileStorageTests
{
    [Fact]
    public async Task Store_open_and_delete_preserve_content_and_use_an_opaque_name()
    {
        var root = Path.Combine(Path.GetTempPath(), "auditarium-fal-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var content = "%PDF-1.7\nexample"u8.ToArray();
            var storage = new LocalFileStorage(root);
            await using var upload = new MemoryStream(content);

            var stored = await storage.StoreAsync(upload, 100, CancellationToken.None);

            Assert.DoesNotContain(".pdf", stored.SaveFileName, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant(), stored.Checksum);
            Assert.True(await storage.ExistsAsync(stored.SaveFilePath, stored.SaveFileName, CancellationToken.None));
            await using var download = await storage.OpenReadAsync(stored.SaveFilePath, stored.SaveFileName, CancellationToken.None);
            using var result = new MemoryStream(); await download.CopyToAsync(result, CancellationToken.None);
            Assert.Equal(content, result.ToArray());

            await storage.DeleteAsync(stored.SaveFilePath, stored.SaveFileName, CancellationToken.None);
            Assert.False(await storage.ExistsAsync(stored.SaveFilePath, stored.SaveFileName, CancellationToken.None));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Store_rejects_an_oversized_stream_without_leaving_a_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "auditarium-fal-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var storage = new LocalFileStorage(root);
            await using var upload = new MemoryStream(new byte[101]);

            await Assert.ThrowsAsync<FileTooLargeException>(() => storage.StoreAsync(upload, 100, CancellationToken.None));

            var directory = Path.Combine(root, "documents", "originals");
            Assert.Empty(Directory.Exists(directory) ? Directory.EnumerateFiles(directory) : []);
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }
}
