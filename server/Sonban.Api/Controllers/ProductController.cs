using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;
using Stripe;
using Stripe.Checkout;

namespace Sonban.Api.Controllers {
    [AuthCookieCheckFilter]
    public class ProductController : BaseApiController {
        private readonly IProductsService productsService;
        private readonly IBooksService booksService;
        private readonly IAudioBooksService audiobooksService;
        private readonly IConfiguration _config;
        private readonly IAccountsService accountsService;

        public ProductController(IProductsService productsService, IBooksService booksService, IAudioBooksService audiobooksService, IConfiguration config, IAccountsService accountsService) {
            this.productsService = productsService;
            this.booksService = booksService;
            this.audiobooksService = audiobooksService;
            _config = config;
            this.accountsService = accountsService;
        }

        [HttpPost, Route("api/user/products/favourite/add")]
        public async Task AddProductToFavourite(ProductDto input) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);
            await productsService.AddProductToFavourite(account.Id, input.ProductId, input.ProductType);
        }

        [HttpPost, Route("api/user/products/favourite/remove")]
        public async Task RemoveProductFromFavourite(ProductDto input) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);
            await productsService.RemoveProductFromFavourite(account.Id, input.ProductId, input.ProductType);
        }

        [HttpPost, Route("api/products/rate")]
        public async Task SetRating(SetRatingDto product) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);
            await productsService.SetRating(product, account.Id, CurrentIdentity.UserEmail);
        }

        [HttpGet("api/files")]
        public async Task<IActionResult> GetProductFile(int productId, int productType) {
            var input = new ProductDto() { ProductId = productId, ProductType = productType };
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);
            var filePath = await productsService.GetProductFilePath(input, account.Id, CurrentIdentity.UserEmail);

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) {
                return NotFound("File is not found.");
            }

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType)) {
                contentType = "application/octet-stream";
            }

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpPost, Route("api/payments/checkout-session")]
        public async Task<ActionResult<object>> CreateCheckoutSession([FromQuery] int productId, [FromBody] CheckoutRequestDto request) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);
            var product = new ProductRequestDto() { Id = productId, ProductType = request.ProductType};
            
            if (request.ProductType == 1) {
                var book = await booksService.GetBookDetails(productId);
                product.Price = book.price;
            }
            else if(request.ProductType == 2) {
                var audiobook = await audiobooksService.GetAudioBookDetails(productId);
                product.Price = audiobook.price;
            }

            product.IsBought = await productsService.IsProductInAccount(product, account.Id);

            if (product.Price == 0) {
                await productsService.AddProductToAccount(product.Id, product.ProductType, account.Id, CurrentIdentity.UserEmail);
                return Ok(new { url = "" });
            }

            if (product.IsBought) {
                return Ok(new { url = "" });
            }

            request.UnitAmount = (long)(product.Price * 100);

            var options = new Stripe.Checkout.SessionCreateOptions {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions> {
                new SessionLineItemOptions {
                    PriceData = new SessionLineItemPriceDataOptions {
                        UnitAmount = request.UnitAmount,
                        Currency = request.Currency,
                        ProductData = new SessionLineItemPriceDataProductDataOptions {
                            Name = request.ProductName,
                        },
                    },
                    Quantity = request.Quantity,
                },
            },
                Mode = "payment",
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,

                Metadata = new Dictionary<string, string> {
                    { "account_id", account.Id.ToString() },
                    { "product_id", productId.ToString() },
                    { "product_type", request.ProductType.ToString() },
                    { "email", CurrentIdentity.UserEmail }
                }
            };

            var service = new Stripe.Checkout.SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { id = session.Id, url = session.Url });
        }

        [AllowAnonymous]
        [HttpPost, Route("api/payments/webhook")]
        public async Task<IActionResult> StripeWebhook() {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _config["Stripe:WebhookSecret"]
                );

                if (stripeEvent.Type == "checkout.session.completed") {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session != null) {
                        var accountId = int.Parse(session.Metadata["account_id"]);
                        var bookId = int.Parse(session.Metadata["product_id"]);
                        var email = session.Metadata["email"];
                        var productType = int.Parse(session.Metadata["product_type"]);//

                        await productsService.AddProductToAccount(bookId, productType, accountId, email);//
                    }
                }

                return Ok();
            }
            catch (StripeException e) {
                return BadRequest();
            }
        }


        [AllowAnonymous]
        [HttpGet, Route("api/comments")]
        public async Task<List<CommentDto>> GetProductComments([FromQuery] ProductDto product) {
            return await productsService.GetProductComments(product);
        }

        [HttpPost, Route("api/comments")]
        public async Task<CommentDto> PostProductComment(AddCommentDto comment) {
            comment.userCreatorId = CurrentIdentity.UserId;
            return await productsService.PostProductComment(comment);
        }
    }
}
