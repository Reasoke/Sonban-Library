using System.Net;
using Dapper;
using Sonban.Api.Dto;
using Sonban.Api.Classes;
using Sonban.Common.Classes;
using Sonban.Common.Dto;
using Stripe.Forwarding;

namespace Sonban.Api.Repository
{
    public interface IUsersRepository
    {
        Task<UserDto> GetUser(int userId);
        Task<UserDto> GetUserByMcpToken(int tokenId);
        Task<UserDto> GetUser(string email, string password);
        Task<bool> IsEmailRegistered(string email);
        Task<UserDto> Register(string name, string email, string password);
        Task ModifyUser(int userId, ModifyUserRequest request);
        Task<string> GeneratePasswordResetToken(string email);
        Task<int> GetUserByResetToken(string token);
        Task<bool> ResetPassword(string token, string password);
        Task ClearPwdTokenCache(string token);
    }

    public class UsersRepository : BaseRepository, IUsersRepository
    {
        public UsersRepository(ISettingsProvider settingsProvider) : base(settingsProvider)
        {
        }

        public async Task<UserDto> GetUser(int userId)
        {
            return await GetConnection().QueryFirstOrDefaultAsync<UserDto>(
                "SELECT userId as Id, Name, Email, isSysAdmin FROM [Users] u WHERE userId = @userId",
                new {userId});
        }

        public async Task<UserDto> GetUser(string email, string password)
        {
            return await GetConnection().QueryFirstOrDefaultAsync<UserDto>(
                "SELECT userId as Id, Name, Email, isSysAdmin FROM [Users] u WHERE lower(Email) = @email AND Password = @password",
                new {email = email.ToLower(), password});
        }

        public async Task<bool> IsEmailRegistered(string email)
        {
            var count = await GetConnection().ExecuteScalarAsync<int>(
                "SELECT userId as Id, Name, Email FROM [Users] u WHERE upper(Email) = @email",
                new {email = email.ToUpper()});
            return count > 0;
        }

        public async Task<UserDto> Register(string name, string email, string password)
        {
            var userId = await GetConnection().ExecuteScalarAsync<int>(
                @"INSERT INTO Users (name, email, password) VALUES (@name, @email, @password);
                DECLARE @UserId INT = SCOPE_IDENTITY();
                insert into Accounts (name, createdTime) values (@name, GETDATE());
                DECLARE @AccountId INT = SCOPE_IDENTITY();
                insert into UsersAccounts (userId, accountId, userTypeId) values (@userId, @accountId, 1);
                Select @UserId;",
                new {name, email, password});
            return new UserDto {Id = userId, Name = name, Email = email};
        }

        public async Task ModifyUser(int userId, ModifyUserRequest request)
        {
            var rowsAffected = await GetConnection().ExecuteAsync(
                "UPDATE [Users] SET name = @name WHERE userId = @userId",
                new {userId, name = request.Name});
            if (rowsAffected == 0)
                throw new DomainException(HttpStatusCode.NotFound, "User is not found");
        }

        public async Task<string> GeneratePasswordResetToken(string email) {
            var token = Guid.NewGuid().ToString().Replace("-", "");
            var rowsAffected = await GetConnection().ExecuteAsync(
                "UPDATE Users SET lastResetPwdToken = @token WHERE email = @email",
                new { email, token });
            if (rowsAffected == 0)
                throw new DomainException(HttpStatusCode.NotFound, "User is not found");
            return token;
        }

        public async Task<int> GetUserByResetToken(string token) {
            return await GetConnection().QueryFirstOrDefaultAsync<int>(
                "SELECT userId as Id FROM Users WHERE lastResetPwdToken = @token",
                new { token });
        }

        public async Task<bool> ResetPassword(string token, string password) {
            var rowsAffected = await GetConnection().ExecuteAsync(
                "UPDATE Users SET password = @password WHERE lastResetPwdToken = @token",
                new { password, token });
            return (rowsAffected != 0);
        }

        public async Task ClearPwdTokenCache(string token) {
            var rowsAffected = await GetConnection().ExecuteAsync(
                "UPDATE Users SET lastResetPwdToken = NULL WHERE lastResetPwdToken = @token",
                new { token });
        }

        public Task<UserDto> GetUserByMcpToken(int tokenId) {
            return GetConnection().QueryFirstOrDefaultAsync<UserDto>(
                @"SELECT u.userId as Id, u.Name, u.Email, u.isSysAdmin FROM [Users] u
    JOIN UserTokens ut ON u.userId = ut.userId
WHERE ut.tokenId = @tokenId",
                new { tokenId });
        }

    }
}
