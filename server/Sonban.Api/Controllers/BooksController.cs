using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;
using Sonban.Common.Dto;

namespace Sonban.Api.Controllers {
    [AuthCookieCheckFilter]
    public class BookController : BaseApiController {
        private readonly IBooksService booksService;
        private readonly IConfiguration _config;
        private readonly IAccountsService accountsService;

        public BookController(IBooksService booksService, IConfiguration config, IAccountsService accountsService) {
            this.booksService = booksService;
            _config = config;
            this.accountsService = accountsService;
        }

        [AllowAnonymous]
        [HttpGet("api/books")]
        public async Task<IEnumerable<BookDto>> GetBooks([FromQuery] string name, [FromQuery] string author, [FromQuery] string genre, [FromQuery] int take = 20, [FromQuery] int skip = 0) {

            return await booksService.GetBooks(take, skip, name, author, genre);
        }

        [AllowAnonymous]
        [HttpGet, Route("api/genres")]
        public async Task<IEnumerable<GenreDto>> GetGenres() {
            var genres = booksService.GetGenres();
            return await genres;
        }

        [HttpGet, Route("api/user/books")]
        public async Task<IEnumerable<BookDto>> GetUsersBooks([FromQuery] string name, [FromQuery] string author, [FromQuery] string genre, [FromQuery] bool? isFavourite, [FromQuery] int take = 20, [FromQuery] int skip = 0) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);

            var books = booksService.GetUsersBooks(take, skip, name, author, genre, isFavourite, CurrentIdentity.UserId, account.Id);
            return await books;
        }

        [HttpGet, Route("api/{accountId:int}/books/favourite")]
        public async Task<IEnumerable<BookDto>> GetFavouriteBooks(int accountId, int take = 20, int skip = 0) {

            var books = booksService.GetFavouriteBooks(take, skip, CurrentIdentity.UserId, CurrentIdentity.UserEmail, accountId);
            return await books;
        }

        [AllowAnonymous]
        [HttpGet, Route("api/books/{bookId:int}")]
        public async Task<BookDetailsDto> GetBookDetails(int bookId) {
            var book = await booksService.GetBookDetails(bookId);

            if (CurrentIdentity != null) {
                var account = await accountsService.GetAccount(CurrentIdentity.UserId);
                book = await booksService.GetBookStatuses(book, account.Id);
            }

            return book;
        }

        [HttpPost("api/admin/uploadbook")]
        public async Task<IActionResult> UploadBook(IFormFile file) {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            var fileExtension = Path.GetExtension(file.FileName);
            if(fileExtension != ".fb2") {
                return BadRequest("File is not fb2 format");
            }

            using (var stream = file.OpenReadStream()) {
                var newbookId = await booksService.UploadBook(stream);

                return Ok(new { bookId = newbookId, message = "File uploaded successfully." });
            }
        }

        [AllowAnonymous]
        [HttpGet("api/books/files/{bookId:int}/cover")]
        public async Task<IActionResult> GetBookCover(int bookId) {
            var filePath = await booksService.GetBookCover(bookId);

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) {
                return NotFound("File is not found.");
            }

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType)) {
                contentType = "image/png";
            }

            var fileName = Path.GetFileName(filePath);
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}
