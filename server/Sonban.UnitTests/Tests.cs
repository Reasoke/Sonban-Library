using Sonban.Api.Services;
using Sonban.Common.Classes;
using UnitTests.Mocks;

namespace Sonban.UnitTests {
    
    public class Tests {
        
        private BooksService booksService;
        [SetUp]
        public void Setup() {
            var settings = new SettingsProviderMock();
            var bookRep = new BooksRepositoryMock();
            var accRep = new AccountsRepositoryMock();
            booksService = new BooksService(bookRep, accRep, settings);
        }

        // GetBooks
        [TestCase("", "", "", 10, 5)]
        [TestCase("", "", "", 100, 10)]
        public async Task GetBooksOkTest(string name, string author, string genre, int take, int skip) {
            var books = await booksService.GetBooks(take, skip, name, author, genre);
            var bookCount = books.ToList().Count;
            Assert.AreEqual(take, bookCount, "books count wrong");
            // Assert.Pass();
        }

        [TestCase("", "", "", -5, 5)]
        [TestCase("", "", "", 20000, 5)]
        public void GetBooksFailTest(string name, string author, string genre, int take, int skip) {
            Assert.CatchAsync<ArgumentException>(async () => await booksService.GetBooks(take, skip, name, author, genre));
        }


        // GetUsersBooks
        [TestCase(10, 5, "", "", "", true, 1, 1)]
        [TestCase(100, 10, "", "", "", true, 1, 1)]
        public async Task GetUsersBooksOkTest(int take, int skip, string name, string author, string genre, bool? isFavourite, int userId, int accountId) {
            var books = await booksService.GetUsersBooks(take, skip, name, author, genre, isFavourite, userId, accountId);
            var bookCount = books.ToList().Count;
            Assert.AreEqual(take, bookCount, "books count wrong");
        }

        [TestCase(-5, 5, "", "", "", true, 1, 1)]
        [TestCase(2000, 5, "", "", "", true, 1, 1)]
        public void GetUsersBooksFailTest(int take, int skip, string name, string author, string genre, bool? isFavourite, int userId, int accountId) {
            Assert.CatchAsync<ArgumentException>(async () => await booksService.GetUsersBooks(take, skip, name, author, genre, isFavourite, userId, accountId));
        }


        // GettFavouriteBooks
        [TestCase(10, 5, 1, "user1", 1)]
        public async Task GetFavouriteBooksOkTest(int take, int skip, int userId, string email, int accountId) {
            var books = await booksService.GetFavouriteBooks(take, skip, userId,email, accountId);
            var bookCount = books.ToList().Count;
            Assert.AreEqual(take, bookCount, "books count wrong");
        }

        [TestCase(-5, 5, 1, "user1", 1)]
        [TestCase(2000, 5, 1, "user1", 1)]
        public void GetFavouriteBooksTakeFailTest(int take, int skip, int userId, string email, int accountId) {
            Assert.CatchAsync<ArgumentException>(async () => await booksService.GetFavouriteBooks(take, skip, userId,email, accountId));
        }
        
        [TestCase(10, 2, 2, "user2", 1)]
        public void GetFavouriteBooksAuthFailTest(int take, int skip, int userId, string email, int accountId) {
            Assert.CatchAsync<DomainException>(async () => await booksService.GetFavouriteBooks(take, skip, userId,email, accountId));
        }
    }
}