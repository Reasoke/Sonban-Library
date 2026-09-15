using System.Net;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Common.Classes;

namespace Sonban.Api.Services {
    public interface IAccountsService {
        Task<AccountDto> GetAccount(int userId);
        Task<IEnumerable<AccountDto>> GetAccounts(int userId);
        Task DeleteAccount(int userId, int accountId);
        Task AddAccount(int userId, AddAccountRequestDto request);
        Task<AccountDetailsDto> GetAccountDetails(int accountId);
        Task EditAccountDetails(AccountDetailsRequest request);
        Task AddAccountUsers(int userId, int accountId, AddAccountUserRequest request);
        Task RemoveAccountUser(int accountId, RemoveAccountUserRequest request);
        Task<AccountInvitesDto> GetAccountInvites(int accountId);
    }

    public class AccountsService : IAccountsService {
        private readonly IAccountsRepository accountsRepository;

        public AccountsService(IAccountsRepository accountsRepository) {
            this.accountsRepository = accountsRepository;
        }

        public async Task<AccountDto> GetAccount(int userId) {
            return await accountsRepository.GetAccount(userId);
        }

        public async Task<IEnumerable<AccountDto>> GetAccounts(int userId) {
            return await accountsRepository.GetAccounts(userId);
        }

        public async Task DeleteAccount(int userId, int accountId) {
            var userAccount = await accountsRepository.GetAccount(userId, accountId);
            if (userAccount == null) {
                throw new DomainException(HttpStatusCode.NotFound, "Account is not found");
            }
            
            switch (userAccount.UserType) {
                case AccountUserType.Admin:
                    await accountsRepository.DeleteAccount(accountId);
                    break;
                case AccountUserType.Member:
                    await accountsRepository.LeaveAccount(userId, accountId);
                    break;
            }
        }

        public async Task AddAccount(int userId, AddAccountRequestDto request) {
            if (request == null || request.Name.IsNullOrEmpty()) {
                throw new DomainException(HttpStatusCode.BadRequest, "Invalid data");
            }

            await accountsRepository.CreateAccount(request.Name, userId);
        }

        public async Task<AccountDetailsDto> GetAccountDetails(int accountId) {
            return await accountsRepository.GetAccountDetails(accountId);
        }

        public async Task EditAccountDetails(AccountDetailsRequest request) {
            if (request == null || request.Name.IsNullOrEmpty()) {
                throw new DomainException(HttpStatusCode.BadRequest, "Invalid data");
            }
            //todo: get previous subscription and compare with new 
            //todo: payment process (or smth like that)
            
            await accountsRepository.ModifyAccount(request);
        }

        public async Task AddAccountUsers(int userId, int accountId, AddAccountUserRequest request) {
            var registered = await accountsRepository.IsUserInAccount(accountId, request.Email);
            if (registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User is already in this account");
            }
            
            var invited = await accountsRepository.IsUserInvitedToAccount(accountId, request.Email);
            if (invited) {
                throw new DomainException(HttpStatusCode.BadRequest, "User is already invited to this account");
            }
            
            await accountsRepository.InviteUser(userId, accountId, request);
        }

        public async Task RemoveAccountUser(int accountId, RemoveAccountUserRequest request) {
            await accountsRepository.DeleteInvite(accountId, request.Email);
        }

        public async Task<AccountInvitesDto> GetAccountInvites(int accountId) {
            return await accountsRepository.GetAccountInvites(accountId);
        }
    }
}