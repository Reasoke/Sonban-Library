using System.Net;
using Sonban.Common.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Classes;

internal static class AuthorizationHelper
{
    private const string AuthCookieName = "SonbanAuth";

    public static void Logout(IRequestCookieCollection requestCookies, IResponseCookies responseCookies)
    {
        if (requestCookies.ContainsKey(AuthCookieName))
        {
            responseCookies.Delete(AuthCookieName);
        }
    }

    public static void Login(UserDto user, IResponseCookies responseCookies)
    {
        var expireAt = DateTime.UtcNow.AddDays(10).ToFileTimeUtc();
        var rawToken = $"{user.Id}-{expireAt}-{Environment.TickCount}"; //todo: add other values
        var token = rawToken.Encrypt();
        responseCookies.Append(AuthCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.Now.AddDays(10), //todo: move days to method parameters
        });
    }

    public static int ValidateCookie(IRequestCookieCollection requestCookies, bool userRequired)
    {
        if (!requestCookies.TryGetValue(AuthCookieName, out var cookie))
        {
            if (userRequired)
            {
                throw new DomainException(HttpStatusCode.Unauthorized, "Unauthorized");
            }
            else
            {
                return -1;
            }
        }

        try
        {
            var decrypted = cookie.Decrypt();
            var tokenValues = decrypted.Split('-');
            if (tokenValues.Length >= 2)
            {
                if (int.TryParse(tokenValues[0], out var userId) &&
                    long.TryParse(tokenValues[1], out var expireAtValue))
                {
                    if (DateTime.FromFileTimeUtc(expireAtValue) < DateTime.UtcNow)
                        throw new DomainException(HttpStatusCode.Unauthorized, "Token expired");
                    return userId;
                }
            }

            throw new DomainException(HttpStatusCode.Unauthorized, "Invalid token content");
        }
        catch
        {
            throw new DomainException(HttpStatusCode.Unauthorized, "Invalid token");
        }
    }
}