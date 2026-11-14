using System.Net;
using Aiursoft.AnduinOSHome.Configuration;
using Aiursoft.AnduinOSHome.Entities;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace Aiursoft.AnduinOSHome.Services.OfflineUpdates;

public sealed class R2UpdateStorage(IOptions<OfflineUpdatesOptions> options) : IOfflineUpdateStorage
{
    private AmazonS3Client CreateClient()
    {
        var settings = options.Value;
        if (!Uri.TryCreate(settings.ServiceUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            string.IsNullOrWhiteSpace(settings.AccessKey) || string.IsNullOrWhiteSpace(settings.SecretKey) ||
            string.IsNullOrWhiteSpace(settings.Bucket))
            throw new InvalidOperationException("Offline update storage is not configured.");
        return new AmazonS3Client(new BasicAWSCredentials(settings.AccessKey, settings.SecretKey),
            new AmazonS3Config { ServiceURL = settings.ServiceUrl, ForcePathStyle = true, AuthenticationRegion = "auto" });
    }

    public async Task<bool> MatchesAsync(OfflineUpdate update, CancellationToken token)
    {
        using var client = CreateClient();
        try
        {
            var metadata = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest
                { BucketName = options.Value.Bucket, Key = update.ObjectKey }, token);
            return metadata.ContentLength == update.Size &&
                   metadata.Metadata["x-amz-meta-sha256"] == update.Sha256 &&
                   metadata.Metadata["x-amz-meta-batch-id"] == update.BatchId;
        }
        catch (AmazonS3Exception error) when (error.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<string> GetDownloadUrlAsync(OfflineUpdate update, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var client = CreateClient();
        return await client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = options.Value.Bucket, Key = update.ObjectKey, Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(1),
            ResponseHeaderOverrides = new ResponseHeaderOverrides
                { ContentDisposition = $"attachment; filename=\"{update.FileName}\"" }
        });
    }
}
