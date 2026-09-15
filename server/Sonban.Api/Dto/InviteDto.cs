namespace Sonban.Api.Dto;

public class InviteDto {
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string GuestEmail { get; set; }
    public AccountUserType UserType { get; set; }
}