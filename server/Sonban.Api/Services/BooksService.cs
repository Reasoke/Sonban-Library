using System.Diagnostics;
using System.Net;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Common.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Services
{
    public interface IBooksService {
        Task<IEnumerable<BookDto>> GetBooks(int take, int skip, string name, string author, string genre);
        Task<IEnumerable<GenreDto>> GetGenres();
        Task<BookDetailsDto> GetBookDetails(int bookId);
        Task<BookDetailsDto> GetBookStatuses(BookDetailsDto book, int accountId);
        Task<IEnumerable<BookDto>> GetUsersBooks(int take, int skip, string name, string author, string genre,bool? isFavourite, int userId, int accountId);
        Task<IEnumerable<BookDto>> GetFavouriteBooks(int take, int skip, int userId, string email, int accountId);
        //Task<string> GetBookFilePath(int bookId, int accountId, string email);
        Task<int> UploadBook(Stream stream);
        Task<string> GetBookCover(int bookId);
        //Task AddBookToAccount(int bookId, int accountId, string email);
    }

    public class BooksService : IBooksService {
        private readonly IBooksRepository booksRepository;
        private readonly IAccountsRepository accountsRepository;
        private readonly string booksPath;
        private readonly string defaultBookCoverPath;
        private readonly string bookConverterPath;

        public BooksService(IBooksRepository booksRepository, IAccountsRepository accountsRepository, ISettingsProvider settingsProvider) {
            this.booksRepository = booksRepository;
            this.accountsRepository = accountsRepository;
            booksPath = settingsProvider.GetValue<string>("BooksPath");
            defaultBookCoverPath = settingsProvider.GetValue<string>("DefaultBookCoverPath");
            bookConverterPath = settingsProvider.GetValue<string>("BookConverterPath");
        }

        public async Task<IEnumerable<BookDto>> GetBooks(int take, int skip, string name, string author, string genre) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            return await booksRepository.GetBooks(take, skip, name, author, genre);
        }

        public async Task<IEnumerable<GenreDto>> GetGenres() {
            return await booksRepository.GetGenres();
        }
        
        public async Task<IEnumerable<BookDto>> GetUsersBooks(int take, int skip, string name, string author, string genre,bool? isFavourite, int userId, int accountId) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            return await booksRepository.GetUsersBooks(take, skip, name, author, genre, accountId, isFavourite);
        }

        public async Task<IEnumerable<BookDto>> GetFavouriteBooks(int take, int skip, int userId, string email, int accountId) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            var registered = await accountsRepository.IsUserInAccount(accountId, email);
            if (!registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
            }

            return await booksRepository.GetFavouriteBooks(take, skip, accountId);
        }

        public async Task<BookDetailsDto> GetBookDetails(int bookId) {
            return await booksRepository.GetBookDetails(bookId);
        }
        
        public async Task<BookDetailsDto> GetBookStatuses(BookDetailsDto book, int accountId) {
            return await booksRepository.GetBookStatuses(book, accountId);
        }

        public async Task<string> GetBookCover(int bookId) {
            var fileName = await booksRepository.GetBookFileName(bookId);
            if (fileName != null) {
                var coverPath = Path.GetFileNameWithoutExtension(fileName);
                coverPath = Path.Combine(booksPath, coverPath, "images");
                if (!Directory.Exists(coverPath)) {
                    var process = Process.Start(new ProcessStartInfo(bookConverterPath, $"{fileName} -epub -preview") {
                        //UseShellExecute = true,
                        CreateNoWindow = true
                    });
                    process.WaitForExit();
                }

                var files = Directory.GetFiles(coverPath, "cover*.*");
                if(files.Length > 0) {
                    return files[0];
                }
                files = Directory.GetFiles(coverPath, "*.*");
                if (files.Length > 0) {
                    return files[0];
                }
            }
            return defaultBookCoverPath;
        }

        public async Task<int> UploadBook(Stream inputStream) {
            var uniqueFileName = Guid.NewGuid().ToString() + ".fb2";
            var filePath = Path.Combine(booksPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create)) {
                await inputStream.CopyToAsync(stream);
            }

            var bookDetails = BookHelper.GetBookDetails(filePath);

            await booksRepository.AddNewBook(bookDetails, uniqueFileName);

            return bookDetails.Id;
        }

        //public async Task AddBookToAccount(int bookId, int accountId, string email) {
        //    var registered = await accountsRepository.IsUserInAccount(accountId, email);
        //    if (!registered) {
        //        throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
        //    }

        //    await booksRepository.AddBookToAccount(bookId, accountId);
        //}
    }
}
