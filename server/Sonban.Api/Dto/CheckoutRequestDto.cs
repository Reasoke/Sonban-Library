namespace Sonban.Api.Dto;

public class CheckoutRequestDto {
    public string ProductName { get; set; } = default!;
    public int ProductType { get; set; }
    public long UnitAmount { get; set; } // in cents(1000 = $10.00)
    public int Quantity { get; set; } = 1;
    public string Currency { get; set; } = "usd";
    public string SuccessUrl { get; set; } = default!;
    public string CancelUrl { get; set; } = default!;
}
