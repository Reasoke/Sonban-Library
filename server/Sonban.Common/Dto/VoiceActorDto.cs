namespace Sonban.Api.Dto {
    public class VoiceActorDto {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => (FirstName + " " + LastName).Trim();
    }
}
