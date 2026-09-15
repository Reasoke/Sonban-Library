using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;

namespace Sonban.Api.Controllers
{
    [AuthCookieCheckFilter]
    public class AccountsController : BaseApiController {
        private readonly IAccountsService accountsService;

        public AccountsController(IAccountsService accountsService) {
            this.accountsService = accountsService;
        }

        [HttpGet, Route("api/account")]
        public async Task<AccountDto> GetAccount() {
            return await accountsService.GetAccount(CurrentIdentity.UserId);
        }
        
        [HttpGet, Route("api/accounts")]
        public async Task<IEnumerable<AccountDto>> GetAccounts() {
            return await accountsService.GetAccounts(CurrentIdentity.UserId);
        }

        [HttpPost, Route("api/accounts")]
        public async Task AddAccount(AddAccountRequestDto request) {
            await accountsService.AddAccount(CurrentIdentity.UserId, request);
        }
        
        [CheckAccount(AccountUserType.Admin)]
        [HttpDelete, Route("api/accounts/{accountId:int}")]
        public async Task DeleteAccount(int accountId) {
            await accountsService.DeleteAccount(CurrentIdentity.UserId, accountId);
        }
        
        [CheckAccount]
        [HttpGet, Route("api/accounts/{accountId:int}")]
        public async Task<AccountDetailsDto> GetAccountDetails(int accountId) {
            return await accountsService.GetAccountDetails(accountId);
        }

        [CheckAccount(AccountUserType.Admin)]
        [HttpPost, Route("api/accounts/{accountId:int}")]
        public async Task EditAccount(AccountDetailsRequest request) {
            await accountsService.EditAccountDetails(request);
        }
        
        [CheckAccount(AccountUserType.Admin)]
        [HttpGet, Route("api/accounts/{accountId:int}/invites")]
        public async Task<AccountInvitesDto> GetInvitesForAccount(int accountId) {
            return await accountsService.GetAccountInvites(accountId);
        }
        
        [CheckAccount(AccountUserType.Admin)]
        [HttpPost, Route("api/accounts/{accountId:int}/users")]
        public async Task AddAccountUsers(int accountId, AddAccountUserRequest request) {
            await accountsService.AddAccountUsers(CurrentIdentity.UserId, accountId, request);
        }
        
        [CheckAccount(AccountUserType.Admin)]
        [HttpDelete, Route("api/accounts/{accountId:int}/users")]
        public async Task RemoveAccountUser(int accountId, RemoveAccountUserRequest request) {
            await accountsService.RemoveAccountUser(accountId, request);
        }
                
        // [HttpDelete, Route("api/accounts/{accountId:int}/users/{userId:int}")]
        // public async Task RemoveAccountUser(int accountId, int userId) {
        //     await accountsService.RemoveAccountUser(CurrentIdentity.UserId, CurrentIdentity.UserEmail, accountId, userId);
        // }
    }
}
