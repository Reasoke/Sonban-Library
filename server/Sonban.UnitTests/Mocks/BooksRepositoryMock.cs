using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Common.Dto;

namespace UnitTests.Mocks;

internal class BooksRepositoryMock : IBooksRepository {
    public Task AddBookToAccount(int bookId, int accountId) {
        throw new NotImplementedException();
    }

    public Task AddBookToFavourite(int accountId, int bookId) {
        throw new NotImplementedException();
    }

    public Task AddNewBook(BookDetailsDto book, string fileName) {
        throw new NotImplementedException();
    }

    public Task<BookDetailsDto> GetBookDetails(int bookId) {
        throw new NotImplementedException();
    }

    public Task<string> GetBookFileName(int accountId) {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BookDto>> GetBooks(int take, int skip, string name, string author, string genre) {
        return Task.FromResult(Enumerable.Range(0, take).Select(i => new BookDto {
            Id = i,
            Authors = new List<AuthorDto> {
                new AuthorDto{
                    Id = i,
                    FirstName = "author" + i,
                    LastName = "aut"
                }
            },
            Name = "Book" + i,
            price = i*10
        }));
    }

    public Task<BookDetailsDto> GetBookStatuses(BookDetailsDto book, int accountId) {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BookDto>> GetFavouriteBooks(int take, int skip, int accountId) {
        return Task.FromResult(Enumerable.Range(0, take).Select(i => new BookDto {
            Id = i,
            Authors = new List<AuthorDto> {
                new AuthorDto{
                    Id = i,
                    FirstName = "author" + i,
                    LastName = "aut"
                }
            },
            Name = "UserBook" + i,
            price = i * 10
        }));
    }

    public Task<IEnumerable<GenreDto>> GetGenres() {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<BookDto>> GetUsersBooks(int take, int skip, string name, string author, string genre, int accountId, bool? isFavourite) {
        return Task.FromResult(Enumerable.Range(0, take).Select(i => new BookDto {
            Id = i,
            Authors = new List<AuthorDto> {
                new AuthorDto{
                    Id = i,
                    FirstName = "author" + i,
                    LastName = "aut"
                }
            },
            Name = "UserBook" + i,
            price = i * 10
        }));
    }

    public Task<bool> IsBookInAccount(int bookId, int accountId) {
        throw new NotImplementedException();
    }

    public Task RemoveBookFromFavourite(int accountId, int bookId) {
        throw new NotImplementedException();
    }

    public Task SetRating(int bookId, int accountId, decimal rating) {
        throw new NotImplementedException();
    }
}