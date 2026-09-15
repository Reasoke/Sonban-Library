using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;
using Sonban.Common.Dto;

namespace Sonban.Api.Controllers
{
    [AuthCookieCheckFilter]
    public class ProfileController : BaseApiController {
            
        private readonly IUsersService usersService;

        public ProfileController(IUsersService usersService) {
            this.usersService = usersService;
        }

        [AllowAnonymous]
        [HttpPost, Route("api/profile/login")]
        public async Task<UserDto> Login(LoginRequest request) {
            var user = await usersService.Login(request.Email, request.Password);
            AuthorizationHelper.Login(user, Response.Cookies);
            return user;
        }

        [AllowAnonymous]
        [HttpPost, Route("api/profile/register")]
        public async Task<UserDto> Register(RegisterRequest request) {
            var user = await usersService.Register(request.Name, request.Email, request.Password);
            AuthorizationHelper.Login(user, Response.Cookies);
            return user;
        }
        
        [AllowAnonymous]
        [HttpGet, Route("api/profile/logout")]
        public IActionResult Logout() {
            
            AuthorizationHelper.Logout(Request.Cookies, Response.Cookies);

            // return RedirectPermanent("http://site.com/#/login");
            return Ok(new {
                response = "logged out",
            });
        }
        
        [HttpGet, Route("api/profile")]
        public async Task<UserDto> GetUser() {
            return await usersService.GetUser(CurrentIdentity.UserId);
        }
        
        [HttpPost, Route("api/profile")]
        public async Task ModifyUser(ModifyUserRequest request) {
            await usersService.ModifyUser(CurrentIdentity.UserId, request);
        }

        //resetting password
        [AllowAnonymous]
        [HttpPost, Route("api/auth/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request) {
            await usersService.GeneratePasswordResetToken(request.Email);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet, Route("api/auth/validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token) {
            var userId = await usersService.GetUserByResetToken(token);
            if (userId == 0)
                return BadRequest();

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost, Route("/api/auth/reset-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordRequest request) {
            await usersService.ResetPassword(request.Token, request.NewPassword);

            return Ok();

        }

        //MCP tokens
        [HttpGet, Route("api/profile/mcp-token")]
        public Task<IEnumerable<McpTokenDto>> GetMcpTokens() => usersService.GetMcpTokens(CurrentIdentity.UserId);

        [HttpPost, Route("api/profile/mcp-token")]
        public Task<string> GenerateMcpToken(string name, DateTime? expirationDate = null) {
            return usersService.GenerateMcpToken(CurrentIdentity.UserId, name, expirationDate);
        }
        
        [HttpDelete, Route("api/profile/mcp-token/{tokenId:int}")]
        public Task DeleteMcpToken(int tokenId) {
            return usersService.DeleteMcpToken(CurrentIdentity.UserId, tokenId);
        }
    }
}
