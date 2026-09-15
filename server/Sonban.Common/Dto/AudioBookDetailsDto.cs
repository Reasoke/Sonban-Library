using Sonban.Common.Dto;
using System.Collections.Generic;

namespace Sonban.Api.Dto {
    public class AudioBookDetailsDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int createdYear { get; set; }
        public int price { get; set; }
        public decimal rating { get; set; }

        public decimal personalRating { get; set; }
        public bool isFavourite { get; set; }
        public bool isBought { get; set; }

        public List<AuthorDto> Authors { get; set; }
        public List<VoiceActorDto> VoiceActors { get; set; }
    }
}
