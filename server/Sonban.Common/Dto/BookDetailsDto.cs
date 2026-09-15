using System.Collections.Generic;

namespace Sonban.Common.Dto {
    public class BookDetailsDto {
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
        public List<GenreDto> Genres { get; set; }
    }
}
