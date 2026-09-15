using System.Net;
using Dapper;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Common.Classes;

namespace Sonban.Api.Repository;

public interface IAccountsRepository {
    Task<IEnumerable<AccountDto>> GetAccounts(int userId);
    Task<AccountDto> GetAccount(int userId, int accountId);
    Task<AccountDto> GetAccount(int userId);
    Task LeaveAccount(int userId, int accountId);
    Task DeleteAccount(int accountId);
    Task CreateAccount(string accountName, int accAdminId);
    Task<AccountDetailsDto> GetAccountDetails(int accountId);
    Task ModifyAccount(AccountDetailsRequest request);
    Task<AccountUserType> GetUserTypeByAccount(int userId, int accountId);
    Task<bool> IsUserInAccount(int accountId, string email);
    Task<bool> IsUserInvitedToAccount(int accountId, string email);
    Task InviteUser(int userId, int accountId, AddAccountUserRequest request);
    Task DeleteInvite(int accountId, string email);
    Task<AccountInvitesDto> GetAccountInvites(int accountId);
}

public class AccountsRepository : BaseRepository, IAccountsRepository {
        
    public AccountsRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
    }

    public async Task<IEnumerable<AccountDto>> GetAccounts(int userId) {
        return await GetConnection().QueryAsync<AccountDto>(
            @"SELECT  A.accountId as Id, A.name, UA.userTypeId as UserType, S.name as Subscription
            FROM UsersAccounts as UA
            JOIN Accounts as A ON UA.accountId = A.accountId
            JOIN Subscriptions as S ON S.subscriptionId = A.subscriptionId
            WHERE UA.userId = @userId
            ORDER BY A.name",
         new { userId });
    }

    public async Task<AccountDto> GetAccount(int userId) {
        var data = await GetConnection().QueryFirstOrDefaultAsync<AccountDto>(
            @"SELECT  A.accountId as Id, A.name, UA.userTypeId as UserType, S.name as Subscription
            FROM UsersAccounts as UA
            JOIN Accounts as A ON UA.accountId = A.accountId
            JOIN Subscriptions as S ON S.subscriptionId = A.subscriptionId
            WHERE UA.userId = @userId", 
            new { userId});
        if (data == null)
            throw new ArgumentException($"Account is not found for userId: {userId}.");
        return data;
    }

    public async Task<AccountDto> GetAccount(int userId, int accountId) {
        return await GetConnection().QueryFirstOrDefaultAsync<AccountDto>(
            @"SELECT  A.accountId as Id, A.name, UA.userTypeId as UserType, S.name as Subscription
            FROM UsersAccounts as UA
            JOIN Accounts as A ON UA.accountId = A.accountId
            JOIN Subscriptions as S ON S.subscriptionId = A.subscriptionId
            WHERE UA.userId = @userId and UA.accountId = @accountId", 
            new { userId, accountId });
    }

    public async Task LeaveAccount(int userId, int accountId) {
        var rowsAffected = await GetConnection().ExecuteAsync(
            @"DELETE FROM UsersAccounts WHERE accountId = @accountId and userId = @userId", 
            new { userId, accountId });
        if (rowsAffected == 0) {
            throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
        }
    }

    //public async Task AddUserToAccount(int userId, int accountId) {
    //    await GetConnection().ExecuteAsync(
    //    @"INSERT INTO UsersAccounts (userId, accountId, userTypeId) values (@adminId, @accountId, @userType);",
    //    new { adminId, userId, userType = AccountUserType.Admin });
    //}

    public async Task DeleteAccount(int accountId) {
        var rowsAffected = await GetConnection().ExecuteAsync(
            @"DELETE FROM Accounts WHERE accountId = @accountId", 
            new { accountId });
        
        if (rowsAffected == 0) {
            throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
        }
    }

    public async Task CreateAccount(string accountName, int accAdminId) {
        await GetConnection().ExecuteAsync(
            @"DECLARE @accountId int;
INSERT INTO Accounts (subscriptionId, name, createdTime, subscriptionExpirationDate) values (1, @accountName, GETDATE(), null);
SELECT @accountId = CAST(SCOPE_IDENTITY() as int);
INSERT INTO UsersAccounts (userId, accountId, userTypeId) values (@adminId, @accountId, @userType);", 
            new { accountName, accAdminId, userType = AccountUserType.Admin });
    }

    public async Task<AccountDetailsDto> GetAccountDetails(int accountId) {
        var reader = await GetConnection().QueryMultipleAsync(
            @"SElECT accountId as Id, subscriptionId as Subscription, name as Name
            FROM Accounts WHERE accountId = @accountId;
SELECT UA.userId as Id, U.name, U.email, UA.userTypeId as UserType
FROM UsersAccounts as UA
JOIN [Users] as U ON U.userId = UA.userId
WHERE UA.accountId = @accountId;", 
            new { accountId });

        var account = await reader.ReadFirstOrDefaultAsync<AccountDetailsDto>();
        if (account == null) {
            throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
        }
        account.AccountUsers = await reader.ReadAsync<UserAccountDto>();

        return account;
    }

    public async Task ModifyAccount(AccountDetailsRequest request) {
        var rowsAffected = await GetConnection().ExecuteAsync(
            @"UPDATE Accounts
SET name = @accountName, subscriptionId = @accountSubscription
WHERE accountId = @accountId;", 
            new { accountId = request.Id, accountName = request.Name, accountSubscription = (int)request.Subscription });
        
        if (rowsAffected == 0) {
            throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
        }
    }

    public async Task<AccountUserType> GetUserTypeByAccount(int userId, int accountId) {
        return await GetConnection().QueryFirstOrDefaultAsync<AccountUserType>(
            @"SELECT userTypeId FROM UsersAccounts WHERE userId = @userId AND accountId = @accountId", 
            new { userId, accountId });
    }

    public async Task<bool> IsUserInAccount(int accountId, string email) {
        var recordsNumberInDB =  await GetConnection().QueryFirstOrDefaultAsync<Int64>(
            @"SELECT 1 FROM UsersAccounts as UA
            JOIN [Users] as U ON UA.userId = U.userId
            WHERE UA.accountId = @accountId AND U.email = @email", 
            new { accountId, email });
        return recordsNumberInDB == 1;
    }

    public async Task<bool> IsUserInvitedToAccount(int accountId, string email) {
        var recordsNumberInDB =  await GetConnection().QueryFirstOrDefaultAsync<Int64>(
            @"SELECT 1 FROM Invites
            WHERE accountId = @accountId AND guestEmail = @email", 
            new { accountId, email });
        return recordsNumberInDB == 1;
    }

    public async Task InviteUser(int userId, int accountId, AddAccountUserRequest request) {
        await GetConnection().ExecuteAsync(
            @"INSERT INTO Invites (accountId, hostUserId, guestEmail, userTypeId, createdTime)
            VALUES (@accountId, @userId, @email, @userTypeId, GETDATE());", 
            new { accountId, userId, email = request.Email, userTypeId = (int)request.UserType });
    }

    public async Task DeleteInvite(int accountId, string email) {
        await GetConnection().ExecuteAsync(
            @"DELETE FROM Invites WHERE accountId = @accountId and guestEmail = @email", 
            new { accountId, email });
    }

    public async Task<AccountInvitesDto> GetAccountInvites(int accountId) {
        var reader = await GetConnection().QueryMultipleAsync(
            @"SElECT accountId as Id, name
            FROM Accounts WHERE accountId = @accountId;
SELECT inviteId as Id, accountId as AccountId, guestEmail as GuestEmail, userTypeId as UserType
            FROM Invites
            WHERE accountId = @accountId;", 
            new { accountId });

        var accountInfo = await reader.ReadFirstOrDefaultAsync<AccountInvitesDto>();
        if (accountInfo == null) {
            throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
        }
        accountInfo.Invites = await reader.ReadAsync<InviteDto>();

        return accountInfo;
    }

}