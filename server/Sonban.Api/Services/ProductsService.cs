using System.Net;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Api.Classes;
using Sonban.Common.Classes;

namespace Sonban.Api.Services
{
    public interface IProductsService {
        Task AddProductToFavourite(int accountId, int productId, int productType);
        Task RemoveProductFromFavourite(int accountId, int productId, int productType);
        Task SetRating(SetRatingDto product, int accountId, string email);
        Task<bool> IsProductInAccount(ProductRequestDto product, int accountId);
        Task AddProductToAccount(int productId, int productType, int accountId, string email);
        Task<string> GetProductFilePath(ProductDto input, int accountId, string email);
        Task<List<CommentDto>> GetProductComments(ProductDto product);
        Task<CommentDto> PostProductComment(AddCommentDto comment);
    }

    public class ProductsService : IProductsService {
        private readonly IProductsRepository productsRepository;
        private readonly IAccountsRepository accountsRepository;
        private readonly string booksPath;
        private readonly string audioBooksPath;

        public ProductsService(IProductsRepository productsRepository, IAccountsRepository accountsRepository, ISettingsProvider settingsProvider) {
            this.productsRepository = productsRepository;
            this.accountsRepository = accountsRepository;
            booksPath = settingsProvider.GetValue<string>("BooksPath");
            audioBooksPath = settingsProvider.GetValue<string>("AudioBooksPath");
        }

        public async Task AddProductToFavourite(int accountId, int productId, int productType) {
            await productsRepository.AddProductToFavourite(accountId, productId, productType);
        }

        public async Task RemoveProductFromFavourite(int accountId, int productId, int productType) {
            await productsRepository.RemoveProductFromFavourite(accountId, productId, productType);
        }

        public async Task<string> GetProductFilePath(ProductDto product, int accountId, string email) {
            var registered = await accountsRepository.IsUserInAccount(accountId, email);
            if (!registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
            }
            var bookInAcc = await productsRepository.IsProductInAccount(product, accountId);
            if (!bookInAcc) {
                throw new DomainException(HttpStatusCode.BadRequest, "This book is not bought");
            }

            var fileName = await productsRepository.GetProductFileName(product);
            if(product.ProductType == 1) {
                return Path.Combine(booksPath, fileName);
            }
            return Path.Combine(audioBooksPath, fileName);
        }

        public async Task SetRating(SetRatingDto product, int accountId, string email) {
            var registered = await accountsRepository.IsUserInAccount(accountId, email);
            if (!registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
            }

            await productsRepository.SetRating(product, accountId);
        }

        public async Task<bool> IsProductInAccount(ProductRequestDto product, int accountId) {
            return await productsRepository.IsProductInAccount(product, accountId);
        }

        public async Task AddProductToAccount(int productId, int productType, int accountId, string email) {
            var registered = await accountsRepository.IsUserInAccount(accountId, email);
            if (!registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
            }

            await productsRepository.AddProductToAccount(productId, productType, accountId);
        }

        public async Task<List<CommentDto>> GetProductComments(ProductDto product) {
            return await productsRepository.GetProductComments(product);
        }

        public async Task<CommentDto> PostProductComment(AddCommentDto comment) {
            var com = await productsRepository.PostProductComment(comment);
            if (com == null) {
                throw new DomainException(HttpStatusCode.BadRequest, "Error adding new comment, try again later");
            }
            return com;
        }
    }
}
