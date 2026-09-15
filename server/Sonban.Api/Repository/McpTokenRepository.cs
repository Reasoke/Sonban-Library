using System.Net;
using Dapper;
using Sonban.Api.Classes;
using Sonban.Common.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Repository
{
    public interface IMcpTokenRepository{
        Task<IEnumerable<McpTokenDto>> GetMcpTokens(int userId);
        Task<int> CreateMcpToken(int userId, string name, DateTime expirationDate);
        Task DeleteMcpToken(int userId, int tokenId);
        Task UpdateLastUsed(int[] tokenIdsToUpdate);
    }

    public class McpTokenRepository: BaseRepository, IMcpTokenRepository {
        
        public McpTokenRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
        }

        public Task<IEnumerable<McpTokenDto>> GetMcpTokens(int userId)
        {
            return GetConnection().QueryAsync<McpTokenDto>(
                "SELECT tokenId as Id, name, expirationDate, lastUsedDate FROM UserTokens WHERE userId=@userId",
                new { userId });
        }

        public async Task<int> CreateMcpToken(int userId, string name, DateTime expirationDate)
        {
            var tokenId = await GetConnection().ExecuteScalarAsync<int>(
                @"INSERT INTO [UserTokens] (userId, name, expirationDate) VALUES (@userId, @name, @expirationDate);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                new { userId, name, expirationDate });
            return tokenId;
        }

        public async Task DeleteMcpToken(int userId, int tokenId)
        {
            var rowsAffected = await GetConnection().ExecuteAsync(
                "DELETE FROM UserTokens WHERE tokenId = @tokenId and userId = @userId", 
                new { tokenId, userId });
            if (rowsAffected == 0) {
                throw new DomainException(HttpStatusCode.NotFound, "Token is not found");
            }
        }

        public Task UpdateLastUsed(int[] tokenIdsToUpdate)
        {
            return GetConnection().ExecuteAsync("UPDATE UserTokens SET lastUsedDate = getutcdate() WHERE tokenId in @tokenIdsToUpdate",
                new { tokenIdsToUpdate });
        }
    }
}