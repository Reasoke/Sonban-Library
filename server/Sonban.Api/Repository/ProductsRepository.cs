using System.Net;
using Dapper;
using Sonban.Api.Dto;
using Sonban.Api.Classes;
using Sonban.Common.Classes;

namespace Sonban.Api.Repository
{
    public interface IProductsRepository {
        Task AddProductToFavourite(int accountId, int productId, int productType);
        Task RemoveProductFromFavourite(int accountId, int productId, int productType);
        Task SetRating(SetRatingDto product, int accountId);
        Task<bool> IsProductInAccount(ProductRequestDto product, int accountId);
        Task<bool> IsProductInAccount(ProductDto product, int accountId);
        Task<string> GetProductFileName(ProductDto product);
        Task AddProductToAccount(int productId, int productType, int accountId);
        Task<List<CommentDto>> GetProductComments(ProductDto product);
        Task<CommentDto> PostProductComment(AddCommentDto comment);
    }

    public class ProductsRepository : BaseRepository, IProductsRepository {
        public ProductsRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
        }

        public async Task AddProductToFavourite(int accountId, int productId, int productType) {
            var rowsAffected = await GetConnection().ExecuteAsync(
            @"UPDATE AccountBooks
SET isFavourite = 1
WHERE accountId = @accountId and objectId = @productId and typeId = @productType;",
            new { accountId, productId, productType});

            if (rowsAffected == 0) {
                throw new DomainException(HttpStatusCode.NotFound, "Book is not found in current account");
            }
        }

        public async Task RemoveProductFromFavourite(int accountId, int productId, int productType) {
            var rowsAffected = await GetConnection().ExecuteAsync(
            @"UPDATE AccountBooks
SET isFavourite = 0
WHERE accountId = @accountId and objectId = @productId and typeId = @productType;",
            new { accountId, productId, productType });

            if (rowsAffected == 0) {
                throw new DomainException(HttpStatusCode.NotFound, "Book is not found in current account");
            }
        }
        
        public async Task SetRating(SetRatingDto product, int accountId) {
            var rowsAffected = await GetConnection().ExecuteAsync(
            @"UPDATE AccountBooks
SET rating = @rating
WHERE accountId = @accountId and objectId = @productId and typeId = @productType",
            new { accountId, productId = product.ProductId, productType= product.ProductType, rating = product.Rating });

            if (rowsAffected == 0) {
                throw new DomainException(HttpStatusCode.NotFound, "Book is not found in current account");
            }
        }

        public async Task<bool> IsProductInAccount(ProductRequestDto product, int accountId) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
                @"SELECT isFavourite FROM AccountBooks where typeId = @productType and accountId = @accountId and objectId = @productId",
                new { productId = product.Id, accountId, productType = product.ProductType });
            return (res != null);
        }

        public async Task<bool> IsProductInAccount(ProductDto product, int accountId) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
                @"SELECT isFavourite FROM AccountBooks where typeId = @productType and accountId = @accountId and objectId = @productId",
                new { productId = product.ProductId, accountId, productType = product.ProductType });
            return (res != null);
        }

        public async Task<string> GetProductFileName(ProductDto product) {
            return await GetConnection().QueryFirstOrDefaultAsync<string>(
                @"SELECT path FROM Files WHERE typeId = @productType and objectId = @productId",
                new { productId = product.ProductId, productType = product.ProductType });
        }

        public async Task AddProductToAccount(int productId, int productType, int accountId) {
            var rowsAffected = await GetConnection().ExecuteAsync(
            @"INSERT INTO AccountBooks (objectId, typeId, accountId) values (@productId, @productType, @accountId);",
            new { accountId, productId, productType });

            if (rowsAffected == 0) {
                throw new DomainException(HttpStatusCode.NotFound, "Book is not found in current account");
            }
        }

        public async Task<List<CommentDto>> GetProductComments(ProductDto product) {
            var result = await GetConnection().QueryAsync<CommentDto>(
            @"SELECT C.commentId, C.objectId as productId, C.typeId as productType, C.message, C.createdTime, 
             C.userCreatorId, U.name AS userCreatorName,
             C.replyToUserId, U2.name AS replyToUserName
      FROM Comments C
      LEFT JOIN Users U ON C.userCreatorId = U.userId
      LEFT JOIN Users U2 ON C.replyToUserId = U2.userId
      WHERE C.objectId = @productId AND C.typeId = @productType
      ORDER BY C.createdTime DESC;",
            new { productId = product.ProductId, productType = product.ProductType });

            return result.ToList();
        }

        public async Task<CommentDto> PostProductComment(AddCommentDto comment) {
            var result = await GetConnection().QuerySingleOrDefaultAsync<CommentDto>(
                    @"DECLARE @newId INT;
          INSERT INTO Comments 
              (objectId, typeId, message, createdTime, userCreatorId, replyToUserId)
          VALUES 
              (@objectId, @typeId, @message, @createdTime, @userCreatorId, NULLIF(@replyToUserId, 0));

          SET @newId = SCOPE_IDENTITY();

          SELECT  C.commentId, C.objectId as productId, C.typeId as productType, C.message, C.createdTime,
              C.userCreatorId, U.name AS userCreatorName,
              C.replyToUserId, U2.name AS replyToUserName
          FROM Comments C
          LEFT JOIN Users U
              ON C.userCreatorId = U.userId
          LEFT JOIN Users U2
              ON C.replyToUserId = U2.userId
          WHERE C.commentId = @newId;",
                    new {
                        objectId = comment.ProductId,
                        typeId = comment.ProductType,
                        message = comment.Message,
                        createdTime = DateTime.Now, // потом лучше UtcNow
                        userCreatorId = comment.userCreatorId,
                        replyToUserId = comment.replyToUserId
                    });
            return result;
        }
    }
}
