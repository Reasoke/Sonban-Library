using System;
using System.Net;

namespace Sonban.Common.Classes
{
    public static class McpTokenValidator
    {
        public static int GetTokenId(string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader))
                throw new DomainException(HttpStatusCode.Forbidden, "No authorization header found");

            var decrypted = authHeader.Decrypt();
            var tokenValues = decrypted.Split('-');
            if (tokenValues.Length < 2) //actually 3
                throw new DomainException(HttpStatusCode.Forbidden, "Invalid token format");

            if (!int.TryParse(tokenValues[0], out var tokenId) ||
                !long.TryParse(tokenValues[1], out var expireAtValue))
                throw new DomainException(HttpStatusCode.Forbidden, "Invalid token values");

            if (DateTime.FromFileTimeUtc(expireAtValue) < DateTime.UtcNow)
                throw new DomainException(HttpStatusCode.Unauthorized, "Token expired");

            return tokenId;
        }
    }
}