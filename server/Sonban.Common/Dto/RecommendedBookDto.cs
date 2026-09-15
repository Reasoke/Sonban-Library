using System.Collections.Generic;

namespace Sonban.Common.Dto {
    public class RecommendedBookDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; } = 0;

        public List<TagDto> Tags { get; set; }

        public List<AuthorDto> Authors { get; set; }
        public List<GenreDto> Genres { get; set; }
    }
}
