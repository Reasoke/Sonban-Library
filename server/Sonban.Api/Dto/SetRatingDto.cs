namespace Sonban.Api.Dto {
    public class SetRatingDto {
        public decimal Rating { get; set; }
        public int ProductId { get; set; }
        public int ProductType { get; set; }
    }
}
