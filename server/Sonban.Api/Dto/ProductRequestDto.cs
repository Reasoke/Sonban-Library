namespace Sonban.Api.Dto;

public class ProductRequestDto {
    public int Id { get; set; }
    public int ProductType { get; set; }
    public decimal Price { get; set; }
    public bool IsBought { get; set; }
}