using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Common.Classes;

namespace Sonban.Api.Classes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CheckAccountAttribute : Attribute, IAsyncActionFilter {
    
    private readonly AccountUserType minRoleRequired;

    public CheckAccountAttribute(AccountUserType minRoleRequired = AccountUserType.Member) {
        this.minRoleRequired = minRoleRequired;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {

        if (!context.ActionArguments.TryGetValue("accountId", out var obj) || obj is not int accountId)
            throw new DomainException(HttpStatusCode.BadRequest, "Missed 'accountId' action argument.");

        if (context.HttpContext.User.Identity is not SonbanUserIdentity identity || !identity.IsAuthenticated)
            throw new DomainException(HttpStatusCode.Unauthorized, "Not authorised");

        var accountsRepository = context.HttpContext.RequestServices.GetService<IAccountsRepository>();
        var userType = await accountsRepository.GetUserTypeByAccount(identity.UserId, accountId);
        if (userType < minRoleRequired)
            throw new DomainException(HttpStatusCode.Forbidden, "Current user has no permission to do this");

        await next();
    }
}