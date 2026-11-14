using Aiursoft.AnduinOSHome.Entities;

namespace Aiursoft.AnduinOSHome.Services.OfflineUpdates;

public interface IOfflineUpdateStorage
{
    Task<bool> MatchesAsync(OfflineUpdate update, CancellationToken token);
    Task<string> GetDownloadUrlAsync(OfflineUpdate update, CancellationToken token);
}
