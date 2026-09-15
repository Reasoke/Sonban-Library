using Dapper;
using Sonban.Api.Dto;
using Sonban.Api.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Repository
{
    public interface IBooksRepository {
        Task<IEnumerable<BookDto>> GetBooks(int take, int skip, string name, string author, string genre);
        Task<IEnumerable<GenreDto>> GetGenres();
        Task<BookDetailsDto> GetBookDetails(int bookId);
        Task<BookDetailsDto> GetBookStatuses(BookDetailsDto book, int accountId);
        Task<IEnumerable<BookDto>> GetFavouriteBooks(int take, int skip, int accountId);
        Task<IEnumerable<BookDto>> GetUsersBooks(int take, int skip, string name, string author, string genre, int accountId, bool? isFavourite);
        Task<string> GetBookFileName(int accountId);
        Task AddNewBook(BookDetailsDto book, string fileName);
        Task<bool> IsBookInAccount(int bookId, int accountId);
        //Task AddBookToAccount(int bookId, int accountId);
    }

    public class BooksRepository : BaseRepository, IBooksRepository {
        public BooksRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
        }

        public async Task<IEnumerable<BookDto>> GetBooks(int take, int skip, string name, string author, string genre) {
            var cache = new List<BookDto>();

            var query = @"WITH cte(bookId, name, price) AS (
    SELECT b.bookId as id, b.name, b.price FROM Books b WITH (NOLOCK)
        LEFT JOIN BooksAuthors BA WITH (NOLOCK) ON b.bookId = BA.bookId
        LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
        LEFT JOIN BooksGenres BG WITH (NOLOCK) ON b.bookId = BG.bookId
        LEFT JOIN Genres G WITH (NOLOCK) ON BG.genreId = G.genreId
    WHERE (@name IS NULL OR b.name LIKE @name)
    AND (@author IS NULL OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) LIKE @author)
    AND (@genre IS NULL OR G.name = @genre)
    GROUP BY b.bookId, b.name, b.price
    ORDER BY b.name
    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
)
SELECT b.bookId as id, b.name, b.price, A.authorId as Id, A.lastName as lastName, A.firstName as firstName
--        , g.name, g.genreId
FROM cte b WITH (NOLOCK)
LEFT JOIN BooksAuthors BA WITH (NOLOCK) ON b.bookId = BA.bookId
LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
-- LEFT JOIN BooksGenres BG WITH (NOLOCK) ON b.bookId = BG.bookId
-- LEFT JOIN Genres G WITH (NOLOCK) ON BG.genreId = G.genreId";

            using var cn = GetConnection();
            await cn.QueryAsync<BookDto, AuthorDto, BookDto>(
                query,
                (book, authorObj) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        cache.Add(foundBook);
                    }
                    if (authorObj != null && foundBook.Authors.All(a => a.Id != authorObj.Id)) {
                        foundBook.Authors.Add(authorObj);
                    }
                    return foundBook;
                },
                new {
                    skip,
                    take,
                    name = name != null ? $"%{name}%" : null,
                    author = author != null ? $"%{author}%" : null,
                    genre
                },
                splitOn: "Id"
            );

            return cache;
        }

        public async Task<IEnumerable<GenreDto>> GetGenres() {
            return await GetConnection().QueryAsync<GenreDto>(
                @"SELECT genreId as Id, name FROM Genres");
        }

        public async Task<IEnumerable<BookDto>> GetUsersBooks(int take, int skip, string name, string author, string genre, int accountId, bool? isFavourite) {
            var cache = new List<BookDto>();
            await GetConnection().QueryAsync<BookDto, AuthorDto,BookDto>(
                @"WITH cte(bookId, name, price) AS (
    SELECT b.bookId as id, b.name, b.price FROM Books b
        JOIN AccountBooks UB on b.bookId = UB.objectId and UB.typeId = 1 and UB.accountId=@accountId
        LEFT JOIN BooksAuthors BA ON b.bookId = BA.bookId
        LEFT JOIN Authors A ON A.authorId = BA.authorId
        LEFT JOIN BooksGenres BG ON b.bookId = BG.bookId
        LEFT JOIN Genres G ON BG.genreId = G.genreId
    WHERE (@name IS NULL OR b.name LIKE @name)
    AND (@author IS NULL OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) LIKE @author)
    AND (@genre IS NULL OR G.name = @genre)
    AND (@isFavourite IS NULL OR UB.isFavourite = @isFavourite)
    GROUP BY b.bookId, b.name, b.price
    ORDER BY b.name
    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
)
SELECT b.bookId as id, b.name, b.price, A.authorId as Id, A.lastName as lastName, A.firstName as firstName
--        , g.name, g.genreId
FROM cte b
LEFT JOIN BooksAuthors BA ON b.bookId = BA.bookId
LEFT JOIN Authors A ON A.authorId = BA.authorId
-- LEFT JOIN BooksGenres BG ON b.bookId = BG.bookId
-- LEFT JOIN Genres G ON BG.genreId = G.genreId",
                ((book, author) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        cache.Add(foundBook);
                    }
                    if (author != null) {
                        if (foundBook.Authors.All(a => a.Id != author.Id))
                            foundBook.Authors.Add(author);
                    }

                    return foundBook;
                }),
                new {
                    skip,
                    take,
                    accountId,
                    name = name != null ? $"%{name}%" : null,
                    author = author != null ? $"%{author}%" : null,
                    genre,
                    isFavourite
                },
                splitOn: "Id"
            );
            return cache;
        }

        public async Task<IEnumerable<BookDto>> GetFavouriteBooks(int take, int skip, int accountId) {
            var cache = new List<BookDto>();
            await GetConnection().QueryAsync<BookDto, AuthorDto,BookDto>(
                @"with cte(BOOK_ID, NAME, PRICE) as (
    SELECT b.bookId as id, b.name, b.PRICE FROM Books b
ORDER BY b.bookId
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY)

SELECT b.BOOK_ID as id, b.NAME, b.PRICE,
       A.authorId as Id, A.lastName as lastName, A.firstName as firstName
FROM cte b
LEFT JOIN BooksAuthors BA on b.BOOK_ID = BA.bookId
LEFT JOIN Authors A on A.authorId = BA.authorId
JOIN AccountBooks UB on b.BOOK_ID = UB.objectId and UB.typeId = 1 and UB.accountId=@accountId and UB.isFavourite = 1",
                ((book, author) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        cache.Add(foundBook);
                    }
                    if (author != null) {
                        if (foundBook.Authors.All(a => a.Id != author.Id))
                            foundBook.Authors.Add(author);
                    }

                    return foundBook;
                }),
                new { skip, take, accountId }, splitOn: "Id");
            return cache;
        }

        public async Task<BookDetailsDto> GetBookDetails(int bookId) {
            var cache = new List<BookDetailsDto>();
            await GetConnection().QueryAsync<BookDetailsDto, AuthorDto, GenreDto, BookDetailsDto>(
                @"SELECT b.bookId as id, b.name, b.description,b.createdYear, b.price,
        ROUND(AVG(UB.rating), 1) as rating,
       A.authorId as Id, A.lastName as lastName, A.firstName as firstName,
       G.genreId as Id, G.name
FROM Books b
LEFT JOIN BooksAuthors BA on b.bookId = BA.bookId
LEFT JOIN Authors A on A.authorId = BA.authorId
LEFT JOIN BooksGenres BG ON b.bookId = BG.bookId
LEFT JOIN Genres G ON BG.genreId = G.genreId
LEFT JOIN AccountBooks UB ON b.bookId = UB.objectId and UB.typeId = 1
WHERE b.bookId = @bookId
group by b.bookId, b.name, b.description, b.createdYear, b.price, A.authorId, A.lastName, A.firstName, G.genreId, G.name",
                ((book, author, genre) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        foundBook.Genres = new List<GenreDto>();
                        cache.Add(foundBook);
                    }
                    if (author != null) {
                        if (foundBook.Authors.All(a => a.Id != author.Id))
                            foundBook.Authors.Add(author);
                    }

                    if (genre != null) {
                        if (foundBook.Genres.All(g => g.Id != genre.Id))
                            foundBook.Genres.Add(genre);
                    }

                    foundBook.isFavourite = false;
                    foundBook.isBought = false;

                    return foundBook;
                }),
                new { bookId }, splitOn: "Id");
            return cache.FirstOrDefault();
        
        }        
        
        public async Task<BookDetailsDto> GetBookStatuses(BookDetailsDto book, int accountId) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
                @"SELECT isFavourite, rating FROM AccountBooks where typeId = 1 and accountId = @accountId and objectId = @bookId",
                new { bookId = book.Id, accountId });
            if (res != null) {
                book.isBought = true;
                book.isFavourite = res.isFavourite;
                book.personalRating = res.rating ?? 0;
            }
            return book;
        }

        public async Task<string> GetBookFileName(int bookId) {
            return await GetConnection().QueryFirstOrDefaultAsync<string>(
                @"SELECT path FROM Files WHERE typeId = 1 and objectId = @bookId",
                new { bookId });            
        }

        public async Task AddNewBook(BookDetailsDto book, string fileName) {
            if(book.Description.Count() >= 2000) {
                book.Description = "";
            }
            book.Id = await GetConnection().ExecuteScalarAsync<int>(
                @"insert into Books (name, description) values (@name, @description);
                DECLARE @BookId INT = SCOPE_IDENTITY();
                insert into Objects (objectId, typeId, createdTime) values (@bookId, 1, GETDATE());
                insert into Files (objectId, typeId, path, createdTime) values (@bookId, 1, @path, GETDATE());
                Select @BookId;",
                new { path = fileName, name = book.Name, description = book.Description});

            foreach(var author in book.Authors) {
                await GetConnection().ExecuteAsync(
                @"IF NOT EXISTS (
                    SELECT 1 FROM Authors
                    WHERE firstName Like @authorFirstName and lastName Like @authorLastName
                )
                BEGIN
                    INSERT INTO Authors (firstName, lastName)
                    VALUES (@authorFirstName, @authorLastName);
                END;
                DECLARE @AuthorId INT = (
                    SELECT authorId FROM Authors
                    WHERE firstName Like @authorFirstName and lastName Like @authorLastName
                );
                INSERT INTO dbo.BooksAuthors (authorId, bookId)
                VALUES (@AuthorId, @BookId);",
                new { authorFirstName = author.FirstName, authorLastName = author.LastName, bookId = book.Id });
            }
        }

        public async Task<bool> IsBookInAccount(int bookId, int accountId) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
                @"SELECT isFavourite FROM AccountBooks where typeId = 1 and accountId = @accountId and objectId = @bookId",
                new { bookId, accountId });
            return (res != null);
        }

        //public async Task AddBookToAccount(int bookId, int accountId) {
        //    var rowsAffected = await GetConnection().ExecuteAsync(
        //    @"INSERT INTO AccountBooks (objectId, typeId, accountId) values (@bookId, 1, @accountId);",
        //    new { accountId, bookId });

        //    if (rowsAffected == 0) {
        //        throw new DomainException(HttpStatusCode.NotFound, "Book is not found in current account");
        //    }
        //}
    }
}
