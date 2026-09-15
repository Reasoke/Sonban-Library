using Sonban.Common.Dto;

namespace Sonban.Api.Dto {
    public class BookDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public int price { get; set; }

        public List<AuthorDto> Authors { get; set; }
    }
}
