using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Sonban.Api.Repository;
using Sonban.Common.Classes;

namespace Sonban.Api.Classes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AuthCookieCheckFilterAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var executingEndpoint = context.HttpContext.GetEndpoint();
        var userRequired = !(executingEndpoint != null &&
                             executingEndpoint.Metadata.OfType<AllowAnonymousAttribute>().Any());

        var userId = AuthorizationHelper.ValidateCookie(context.HttpContext.Request.Cookies, userRequired);

        if (userId == -1 && !userRequired)
        {
            return;
        }

        var userRepository = context.HttpContext.RequestServices.GetService<IUsersRepository>();
        var user = await userRepository.GetUser(userId);
        if (user == null)
        {
            throw new DomainException(HttpStatusCode.Forbidden, "User is not found");
        }

        var identity = new SonbanUserIdentity(user.Id, user.Email, user.IsSysAdmin);
        // identity.AddClaim(new Claim(ClaimTypes.Name, user.Email)); //just example
        context.HttpContext.User = new ClaimsPrincipal(identity);
    }
}