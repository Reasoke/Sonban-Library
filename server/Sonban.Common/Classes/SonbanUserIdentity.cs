using System.Security.Claims;

namespace Sonban.Common.Classes;

public class SonbanUserIdentity : ClaimsIdentity
{
    public int UserId { get; }
    public string UserEmail { get; }
    public bool IsSysAdmin { get; }

    public SonbanUserIdentity(int userId, string userEmail, bool isSysAdmin) : 
        base([new Claim("sub", userId.ToString())], "Basic")
    {
        UserId = userId;
        UserEmail = userEmail;
        IsSysAdmin = isSysAdmin;
    }
}
