namespace Sonban.Api.Dto;

public class UserAccountDto {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public AccountUserType UserType { get; set; }
}