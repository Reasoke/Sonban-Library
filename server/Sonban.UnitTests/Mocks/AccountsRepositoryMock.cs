using Sonban.Api.Dto;
using Sonban.Api.Repository;

namespace UnitTests.Mocks;

internal class AccountsRepositoryMock : IAccountsRepository {
    public Task CreateAccount(string accountName, int adminId) {
        throw new NotImplementedException();
    }

    public Task DeleteAccount(int accountId) {
        throw new NotImplementedException();
    }

    public Task DeleteInvite(int accountId, string email) {
        throw new NotImplementedException();
    }

    public Task<AccountDto> GetAccount(int userId, int accountId) {
        throw new NotImplementedException();
    }

    public Task<AccountDto> GetAccount(int userId) {
        throw new NotImplementedException();
    }

    public Task<AccountDetailsDto> GetAccountDetails(int accountId) {
        throw new NotImplementedException();
    }

    public Task<AccountInvitesDto> GetAccountInvites(int accountId) {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AccountDto>> GetAccounts(int userId) {
        throw new NotImplementedException();
    }

    public Task<AccountUserType> GetUserTypeByAccount(int userId, int accountId) {
        throw new NotImplementedException();
    }

    public Task InviteUser(int userId, int accountId, AddAccountUserRequest request) {
        throw new NotImplementedException();
    }

    public Task<bool> IsUserInAccount(int accountId, string email) {
        return Task.FromResult(accountId == 1 && email == "user1");
    }

    public Task<bool> IsUserInvitedToAccount(int accountId, string email) {
        throw new NotImplementedException();
    }

    public Task LeaveAccount(int userId, int accountId) {
        throw new NotImplementedException();
    }

    public Task ModifyAccount(AccountDetailsRequest request) {
        throw new NotImplementedException();
    }
}