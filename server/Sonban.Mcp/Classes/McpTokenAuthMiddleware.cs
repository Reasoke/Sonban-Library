using System.Net;
using Microsoft.AspNetCore.Http;
using Sonban.Common.Classes;

namespace Sonban.Mcp.Classes;

public class McpTokenAuthMiddleware
{
    private readonly RequestDelegate next;

    public McpTokenAuthMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var tokenId = McpTokenValidator.GetTokenId(context.Request.Headers.Authorization.ToString());

            //todo: use {tokenId}
            // UserDto user = await mcpTokenRepository.GetUserByToken(tokenId);
            // var identity = new SonbanMcpTokenIdentity(user.Id, user.Email, user.IsSysAdmin, authHeader);
            // context.User = new ClaimsPrincipal(identity);
            
            await next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex is DomainException domainException 
                ? (int)domainException.ErrorCode
                : (int)HttpStatusCode.Forbidden;
            var result = System.Text.Json.JsonSerializer.Serialize(new { Message = $"Invalid token: {ex.Message}"});
            await context.Response.WriteAsync(result);
        }
    }
}