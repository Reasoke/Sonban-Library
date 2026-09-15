using System.Net;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Common.Classes;
using Sonban.Common.Dto;
using Stripe.Forwarding;

namespace Sonban.Api.Services
{
    public interface IUsersService {
        Task<UserDto> GetUser(int userId);
        Task<UserDto> Login(string email, string password);
        Task<UserDto> Register(string name, string email, string password);
        Task ModifyUser(int userId, ModifyUserRequest request);
        Task<IEnumerable<McpTokenDto>> GetMcpTokens(int userId);
        Task<string> GenerateMcpToken(int userId, string tokenName, DateTime? expirationDate);
        Task DeleteMcpToken(int userId, int tokenId);
        Task GeneratePasswordResetToken(string email);
        Task<int> GetUserByResetToken(string token);
        Task ResetPassword(string token, string password);
    }

    public class UsersService : IUsersService {

        private readonly IUsersRepository usersRepository;
        private readonly IMcpTokenRepository mcpTokenRepository;
        private readonly IEmailService emailService;
        private readonly string appSiteUrl;

        public UsersService(IUsersRepository usersRepository, IMcpTokenRepository mcpTokenRepository, 
            IEmailService emailService, ISettingsProvider settingsProvider) {
            this.usersRepository = usersRepository;
            this.mcpTokenRepository = mcpTokenRepository;
            this.emailService = emailService;
            appSiteUrl = settingsProvider.GetValue<string>("appSiteUrl");
        }

        public async Task<UserDto> GetUser(int userId) {
            return await usersRepository.GetUser(userId);
        }

        public async Task<UserDto> Login(string email, string password) {
            var hashedPassword = password.GetHash();
            var user = await usersRepository.GetUser(email, hashedPassword);
            if (user == null)
                throw new DomainException(HttpStatusCode.Forbidden, "User is not found");
            return user;
        }
        
        public async Task<UserDto> Register(string name, string email, string password) {
            //todo (or not): check if it's mail
            var registered = await usersRepository.IsEmailRegistered(email);
            if (registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User is already registered");
            }
            
            var hashedPassword = password.GetHash();
            //var name = "user";//todo: change
            var user = await usersRepository.Register(name, email, hashedPassword);
            if (user == null) {
                throw new DomainException(HttpStatusCode.BadRequest, "Error registering User, try again later");
            }

            //await accountsRepository.CreateAccount(name, user.Id);

            return user;
        }

        public async Task ModifyUser(int userId, ModifyUserRequest request) {
            await usersRepository.ModifyUser(userId, request);
        }

        //resetting password
        public async Task GeneratePasswordResetToken(string email) {
            try {
                var token = await usersRepository.GeneratePasswordResetToken(email);
                var messageBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8'>
  <title>Password Reset</title>
</head>

<body style='margin:0;padding:0;background-color:#f4f7fa;font-family:Arial,sans-serif;'>

  <table role='presentation' width='100%' style='padding:40px 0;'>
    <tr>
      <td align='center'>

        <table role='presentation' width='600' style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

          <!-- HEADER -->
          <tr>
            <td style='background:#5783C8;padding:20px;text-align:center;color:white;font-size:20px;font-weight:bold;'>
              Sonban Team
            </td>
          </tr>

          <!-- BODY -->
          <tr>
            <td style='padding:30px;text-align:center;'>
              
              <h2 style='color:#1c2d3f;margin-bottom:10px;'>
                Password Reset Request
              </h2>

              <p style='color:#3d4d5f;font-size:15px;line-height:1.5;'>
                We received a request to reset your password.
                If this was you, click the button below.
              </p>

              <a href='{appSiteUrl}/reset-password/{token}'
                 style='display:inline-block;margin-top:20px;padding:12px 24px;
                        background:#5783C8;color:#fff;text-decoration:none;
                        border-radius:8px;font-weight:bold;'>
                Reset Password
              </a>

              <p style='margin-top:25px;font-size:12px;color:#888;line-height:1.4;'>
                If you didn’t request this, you can safely ignore this email.
              </p>

            </td>
          </tr>

          <!-- FOOTER -->
          <tr>
            <td style='background:#f0f3f6;text-align:center;padding:15px;font-size:12px;color:#777;'>
              © {DateTime.UtcNow.Year} Sonban Team. All rights reserved.
            </td>
          </tr>

        </table>

      </td>
    </tr>
  </table>

</body>
</html>
";
                await emailService.SendMail(email, "Reset your password", messageBody);
            }
            catch {
                //ignore
            }
        }

        public async Task<int> GetUserByResetToken(string token) {
            return await usersRepository.GetUserByResetToken(token);
        }

        public async Task ResetPassword(string token, string password) {
            var hashedPassword = password.GetHash();
            var pwdChanged = await usersRepository.ResetPassword(token, hashedPassword);
            if (!pwdChanged) {
                throw new DomainException(HttpStatusCode.BadRequest, "Error while changing password, try again later");
            }
            await usersRepository.ClearPwdTokenCache(token);
        }

        //MCP tokens
        public Task<IEnumerable<McpTokenDto>> GetMcpTokens(int userId)
        {
            return mcpTokenRepository.GetMcpTokens(userId);
        }

        public async Task<string> GenerateMcpToken(int userId, string tokenName, DateTime? expirationDate)
        {
            if (string.IsNullOrWhiteSpace(tokenName))
                throw new DomainException(HttpStatusCode.BadRequest, "Token name is required");
            if (expirationDate.HasValue &&  expirationDate.Value < DateTime.UtcNow)
                throw new DomainException(HttpStatusCode.BadRequest, "Expiration date must be after current date");
            expirationDate ??= DateTime.UtcNow.AddMonths(1);
            
            var tokenId = await mcpTokenRepository.CreateMcpToken(userId, tokenName, expirationDate.Value);
            
            var expireAt = expirationDate.Value.ToFileTimeUtc();
            var rawToken = $"{tokenId}-{expireAt}-{Environment.TickCount}"; //todo: add other values
            var token = rawToken.Encrypt();

            return token;
        }

        public Task DeleteMcpToken(int userId, int tokenId)
        {
            return mcpTokenRepository.DeleteMcpToken(userId, tokenId);
        }


    }
}
