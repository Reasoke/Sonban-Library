using Sonban.Common.Dto;

namespace Sonban.Api.Dto {
    public class AudioBookDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public int price { get; set; }

        public List<AuthorDto> Authors { get; set; }
        public List<VoiceActorDto> VoiceActors { get; set; }
    }
}
