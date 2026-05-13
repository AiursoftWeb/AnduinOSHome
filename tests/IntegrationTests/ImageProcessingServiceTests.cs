using Aiursoft.AnduinOSHome.Services.FileStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;

namespace Aiursoft.AnduinOSHome.Tests.IntegrationTests;

[TestClass]
public class ImageProcessingServiceTests
{
    private ImageProcessingService _service = null!;
    private StorageService _storage = null!;
    private string _tempPath = null!;

    [TestInitialize]
    public void Initialize()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "AnduinOSHomeImageProcessingTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Storage:Path", _tempPath },
                { "Storage:Key", "test-key" }
            })
            .Build();

        var rootProvider = new StorageRootPathProvider(config);
        var foldersProvider = new FeatureFoldersProvider(rootProvider);
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var fileLockProvider = new FileLockProvider(memoryCache);
        _storage = new StorageService(foldersProvider, fileLockProvider, new EphemeralDataProtectionProvider());
        _service = new ImageProcessingService(
            foldersProvider,
            _storage,
            NullLogger<ImageProcessingService>.Instance,
            fileLockProvider);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [TestMethod]
    public async Task CompressWidthOnlyPreservesAspectRatio()
    {
        var logicalPath = CreateTestImage("images/wide.png", 800, 400);
        var result = await _service.CompressAsync(logicalPath, 400, 0);

        using var bitmap = SKBitmap.Decode(result);
        Assert.AreEqual(400, bitmap.Width);
        Assert.AreEqual(200, bitmap.Height);
    }

    [TestMethod]
    public async Task CorruptImageReturnsOriginalPhysicalPath()
    {
        var logicalPath = "images/corrupt.png";
        var physicalPath = _storage.GetFilePhysicalPath(logicalPath);
        var directory = Path.GetDirectoryName(physicalPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);
        await File.WriteAllBytesAsync(physicalPath, [0x89, 0x50, 0x4E, 0x47, 0x00]);

        var compressResult = await _service.CompressAsync(logicalPath, 100, 0);
        var clearResult = await _service.ClearExifAsync(logicalPath);

        Assert.AreEqual(physicalPath, compressResult);
        Assert.AreEqual(physicalPath, clearResult);
    }

    private string CreateTestImage(string logicalPath, int width, int height)
    {
        var physicalPath = _storage.GetFilePhysicalPath(logicalPath);
        var directory = Path.GetDirectoryName(physicalPath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.LightBlue);
        using (var paint = new SKPaint())
        {
            paint.Color = SKColors.DarkBlue;
            paint.IsAntialias = true;
            canvas.DrawRect(0, 0, width / 2f, height / 2f, paint);
        }
        canvas.Flush();

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Failed to encode test image.");
        using var stream = File.Create(physicalPath);
        data.SaveTo(stream);

        return logicalPath;
    }
}
