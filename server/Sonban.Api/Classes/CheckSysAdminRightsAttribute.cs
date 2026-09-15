using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Sonban.Common.Classes;

namespace Sonban.Api.Classes;

public class CheckSysAdminRightsAttribute : Attribute, IAsyncActionFilter {

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {

        if (context.HttpContext.User.Identity is not SonbanUserIdentity identity || !identity.IsAuthenticated)
            throw new DomainException(HttpStatusCode.Unauthorized, "Not authorised");

        if (!identity.IsSysAdmin)
            throw new DomainException(HttpStatusCode.Forbidden, "Current user has no system admin rights");

        await next();
    }
}