namespace Sonban.Api.Dto;

public class AccountInvitesDto {
    public int Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<InviteDto> Invites { get; set; }
}