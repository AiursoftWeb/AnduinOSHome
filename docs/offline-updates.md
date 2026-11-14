# Offline update downloads

`/pro/offline-update-package` lists published `.atgz` bundles. It reuses the site's shared navigation and footer and shows an empty state until an operator publishes a bundle. Downloads are temporarily anonymous; the route is not promoted in the navigation and is not a secret. There are no activation checks or search-engine access controls.

## Configuration

Set the following through the application's existing deployment configuration. Keep credentials outside source control:

| Setting | Meaning |
| --- | --- |
| `OfflineUpdates:PublisherToken` | Shared token for the trusted bundle publisher; empty disables publication |
| `OfflineUpdates:ServiceUrl` | HTTPS R2 S3 endpoint for the selected jurisdiction |
| `OfflineUpdates:Bucket` | Bucket containing offline bundles |
| `OfflineUpdates:AccessKey` | R2 S3 read access key ID |
| `OfflineUpdates:SecretKey` | Corresponding R2 secret |

Environment variable names use `__` in place of `:`, as with other ASP.NET Core settings. The website only needs read access; the publisher needs read/write access. This change supplies no credentials and creates no production bucket, scheduler or release. Apply the ordinary SQLite or MySQL EF migration when deploying.

## Publication contract

The trusted publisher sends `POST /api/offline-updates` with `Authorization: Bearer <publisher token>` and JSON:

```json
{
  "batchId": "64 lowercase hex characters",
  "releaseLine": "2.0",
  "suite": "resolute",
  "architecture": "amd64",
  "targetVersion": "2.0.5",
  "minimumVersion": "2.0.0",
  "sha256": "64 lowercase hex characters",
  "signingFingerprint": "40 or 64 uppercase hex characters",
  "size": 1619795997,
  "createdUtc": "2026-11-14T00:00:00Z",
  "summary": "Security and reliability updates."
}
```

The hex descriptions above are placeholders. Use verified archive metadata. `architecture` supports `amd64` and `arm64`; source and target must share a release line, and the source cannot be newer. No arbitrary object key or redirect URL is accepted.

Before registration, Home checks the object's size and `sha256` / `batch-id` S3 metadata at `offline/{suite}/{releaseLine}/{architecture}/{batchId}/{sha256}.atgz`. The publisher is responsible for checking signatures, full stored bytes and installation qualification. Home does not claim that an S3 metadata check independently validates the archive signature.

Responses: `201` new batch, `200` identical retry, `409` conflicting or withdrawn batch / missing object, `400` invalid metadata, `401` missing or wrong token, `503` unavailable storage. The database key makes concurrent duplicate registration safe. No record is written before the object check succeeds.

`DELETE /api/offline-updates/{batchId}` with the same token withdraws a batch (`204`, or `404` if unknown). Retrying withdrawal is safe; a withdrawn batch cannot be republished under the same ID. This endpoint does not delete storage objects.

The page shows the latest three active batches per release line, suite and architecture, ordered by numeric target patch and then archive creation time. Published time is not used to promote delayed old builds. Display limiting does not implement physical retention.

Each download checks the object again and redirects to a one-hour R2 URL with attachment disposition. Redirect responses are not cached. Missing or withdrawn entries return `404`; unavailable storage returns `503`. Previously issued R2 links can remain valid until expiry after withdrawal. The updater independently verifies signatures when opening the file.

## Validation boundaries

Business tests exercise publication retries, conflicts, validation, withdrawal, ordering and escaped summaries with isolated records and fake storage. Actual R2 credentials and a production-signed, installation-qualified bundle are required for live end-to-end acceptance. A passing unit test is not evidence of a production upload or a working 48-hour schedule.
