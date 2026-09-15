namespace Sonban.Api.Dto;

public class AddAccountUserRequest {
    public string Email { get; set; }
    public AccountUserType UserType { get; set; }
}