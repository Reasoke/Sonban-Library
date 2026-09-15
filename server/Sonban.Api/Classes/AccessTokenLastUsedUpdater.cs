using Sonban.Api.Repository;

namespace Sonban.Api.Classes;

public interface IAccessTokenLastUsedUpdater
{
    void Post(int tokenId);
}

public class AccessTokenLastUsedUpdater: IAccessTokenLastUsedUpdater
{
    private const int UpdateInterval = 1 * 60 * 1000; //1 minute (in milliseconds)
    private readonly IMcpTokenRepository mcpTokenRepository;
    private readonly ILogger logger;
    private readonly Dictionary<int, DateTime> activeTokens = new();

    public AccessTokenLastUsedUpdater(IMcpTokenRepository mcpTokenRepository, ILogger logger) {
    
        this.mcpTokenRepository = mcpTokenRepository;
        this.logger = logger;
        Task.Factory.StartNew(UpdateLastUsedRoutine, TaskCreationOptions.LongRunning);
    }

    public void Post(int tokenId) {

        lock(activeTokens)
            activeTokens[tokenId] = DateTime.UtcNow; //value never used
    }

    private async Task UpdateLastUsedRoutine() {

        logger.LogInformation("UpdateLastUsedRoutine started");
        while (true) {
            await Task.Delay(UpdateInterval);

            int[] idsToUpdate;
            lock (activeTokens) {
                idsToUpdate = activeTokens.Select(p => p.Key).ToArray();
                activeTokens.Clear();
            }
            if (idsToUpdate.Length > 0) {
                try {
                    await mcpTokenRepository.UpdateLastUsed(idsToUpdate).ConfigureAwait(false);
                }
                catch (Exception ex) {
                    logger.LogError(ex, "Error updating token(s) last used time");
                }
            }
        }
    }
}