using Dapper;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Common.Dto;

namespace Sonban.Api.Repository
{
    public interface IMcpRepository {
        Task<bool> IsBookTagged(int productId, int productType);
        Task<bool> ParseBookForTags(SaveBookTagsRequest request);
        Task<List<RecommendedBookDto>> GetBooksFromUserLibraryForRecommendations(int accountId);
        Task<List<RecommendedBookDto>> SearchBooksByRecommendations(RecommendationBookCriteriaDto criteria);
        Task<List<RecommendedAudioBookDto>> GetAudioBooksFromUserLibraryForRecommendations(int userId);
        Task<List<RecommendedAudioBookDto>> SearchAudioBooksByRecommendations(RecommendationAudioBookCriteriaDto criteria);
    }

    public class McpRepository : BaseRepository, IMcpRepository {
        public McpRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
        }

        public async Task<bool> IsBookTagged(int productId, int productType) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
            @"SELECT productTagId FROM ProductTags where productId = @productId and productTypeId = @productType",
            new { productId, productType });
            return (res != null);
        }

        public async Task<bool> ParseBookForTags(SaveBookTagsRequest request) {
            var connection = GetConnection();

            var parsedTags = request.Tags
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();

            if (!parsedTags.Any())
                return false;

            foreach (var tag in parsedTags) {
                // ищем tagId
                var tagId = await connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT tagId 
              FROM Tags 
              WHERE name = @tag",
                    new { tag });

                // если нет — создаём
                if (tagId == null) {
                    tagId = await connection.QuerySingleAsync<int>(
                        @"INSERT INTO Tags(name)
                  OUTPUT INSERTED.tagId
                  VALUES(@tag)",
                        new { tag });
                }

                // проверяем связь
                var exists = await connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*)
              FROM ProductTags
              WHERE productId = @productId
                AND productTypeId = @productType
                AND tagId = @tagId",
                    new {
                        request.ProductId,
                        request.ProductType,
                        tagId
                    });

                // создаём связь
                if (exists == 0) {
                    await connection.ExecuteAsync(
                        @"INSERT INTO ProductTags
                  (productId, productTypeId, tagId)
                  VALUES
                  (@productId, @productType, @tagId)",
                        new {
                            request.ProductId,
                            request.ProductType,
                            tagId
                        });
                }
            }

            return true;
        }

        public async Task<List<RecommendedBookDto>> GetBooksFromUserLibraryForRecommendations(int accountId) {
            var cache = new List<RecommendedBookDto>();
            await GetConnection().QueryAsync<RecommendedBookDto, AuthorDto, GenreDto, TagDto, RecommendedBookDto>(
                @"with cte as (
    SELECT TOP 10 b.bookId as id, b.name
FROM Books b
JOIN AccountBooks UB 
    ON UB.objectId = b.bookId 
    AND UB.typeId = 1
    AND UB.accountId = @accountId
ORDER BY UB.rating DESC)

SELECT b.id, b.name,
        A.authorId as Id, A.lastName as lastName, A.firstName as firstName,
        G.genreId as Id, G.name,
        T.tagId as Id, T.name
FROM cte b
LEFT JOIN BooksAuthors BA on b.Id = BA.bookId
LEFT JOIN Authors A on A.authorId = BA.authorId
LEFT JOIN BooksGenres BG on BG.bookId = b.Id
LEFT JOIN Genres G on G.genreId = BG.genreId
LEFT JOIN ProductTags PT on PT.productId = b.Id AND PT.productTypeId = 1
LEFT JOIN Tags T on T.tagId = PT.tagId",
                ((book, author, genre, tag) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        foundBook.Genres = new List<GenreDto>();
                        foundBook.Tags = new List<TagDto>();
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

                    if (tag != null) {
                        if (foundBook.Tags.All(t => t.Id != tag.Id))
                            foundBook.Tags.Add(tag);
                    }

                    return foundBook;
                }),
                new { accountId }, splitOn: "Id");
            return cache;
        }

        public async Task<List<RecommendedBookDto>> SearchBooksByRecommendations(RecommendationBookCriteriaDto criteria) {
            var cache = new List<RecommendedBookDto>();
            await GetConnection().QueryAsync<RecommendedBookDto, AuthorDto, GenreDto, TagDto, RecommendedBookDto>(
                @"WITH AuthorScores AS (
    SELECT
        b.bookId,
        COUNT(DISTINCT A.authorId) * 5 AS AuthorScore
    FROM Books b
    JOIN BooksAuthors BA ON BA.bookId = b.bookId
    JOIN Authors A ON A.authorId = BA.authorId
    WHERE A.firstName IN @authors OR A.lastName IN @authors OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) IN @authors
    GROUP BY b.bookId
),

GenreScores AS (
    SELECT
        b.bookId,
        COUNT(DISTINCT G.genreId) * 3 AS GenreScore
    FROM Books b
    JOIN BooksGenres BG ON BG.bookId = b.bookId
    JOIN Genres G ON G.genreId = BG.genreId
    WHERE G.name IN @genres
    GROUP BY b.bookId
),

TagScores AS (
    SELECT
        b.bookId,
        COUNT(DISTINCT T.tagId) * 2 AS TagScore
    FROM Books b
    JOIN ProductTags PT
        ON PT.productId = b.bookId
        AND PT.productTypeId = 1
    JOIN Tags T
        ON T.tagId = PT.tagId
    WHERE T.name IN @tags
    GROUP BY b.bookId
),

BookScores AS (
    SELECT
        b.bookId,
        b.name,

        ISNULL(a.AuthorScore, 0)
        + ISNULL(g.GenreScore, 0)
        + ISNULL(t.TagScore, 0)
        AS Score

    FROM Books b

    LEFT JOIN AuthorScores a ON a.bookId = b.bookId
    LEFT JOIN GenreScores g ON g.bookId = b.bookId
    LEFT JOIN TagScores t ON t.bookId = b.bookId

    WHERE
        ISNULL(a.AuthorScore, 0) > 0
        OR ISNULL(g.GenreScore, 0) > 0
        OR ISNULL(t.TagScore, 0) > 0
),

TopBooks AS (
    SELECT TOP 10 *
    FROM BookScores
    ORDER BY Score DESC
)

SELECT
    TB.bookId AS Id,
    TB.name,
    TB.Score,

    A.authorId AS Id,
    A.firstName,
    A.lastName,

    G.genreId AS Id,
    G.name,

    T.tagId AS Id,
    T.name

FROM TopBooks TB

LEFT JOIN BooksAuthors BA
    ON BA.bookId = TB.bookId

LEFT JOIN Authors A
    ON A.authorId = BA.authorId

LEFT JOIN BooksGenres BG
    ON BG.bookId = TB.bookId

LEFT JOIN Genres G
    ON G.genreId = BG.genreId

LEFT JOIN ProductTags PT
    ON PT.productId = TB.bookId
    AND PT.productTypeId = 1

LEFT JOIN Tags T
    ON T.tagId = PT.tagId

ORDER BY TB.Score DESC;",
                ((book, author, genre, tag) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        foundBook.Genres = new List<GenreDto>();
                        foundBook.Tags = new List<TagDto>();
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

                    if (tag != null) {
                        if (foundBook.Tags.All(t => t.Id != tag.Id))
                            foundBook.Tags.Add(tag);
                    }

                    return foundBook;
                }),
                new { authors = criteria.Authors, genres = criteria.Genres, tags = criteria.Tags }, splitOn: "Id");
            return cache;
        }


        public async Task<List<RecommendedAudioBookDto>> GetAudioBooksFromUserLibraryForRecommendations(int accountId) {
            var cache = new List<RecommendedAudioBookDto>();
            await GetConnection().QueryAsync<RecommendedAudioBookDto, AuthorDto, VoiceActorDto, TagDto, RecommendedAudioBookDto>(
                @"with cte as (
    SELECT TOP 10 ab.audioBookId as id, ab.name
FROM AudioBooks ab
JOIN AccountBooks UB
    ON UB.objectId = ab.audioBookId
    AND UB.typeId = 2
    AND UB.accountId = @accountId
ORDER BY UB.rating DESC)

SELECT ab.id, ab.name,
        A.authorId as Id, A.lastName as lastName, A.firstName as firstName,
        VA.voiceActorId as Id, VA.lastName as lastName, VA.firstName as firstName,
        T.tagId as Id, T.name
FROM cte ab
LEFT JOIN AudioBooksAuthors BA on ab.Id = BA.audioBookId
LEFT JOIN Authors A on A.authorId = BA.authorId
LEFT JOIN AudioBooksVoiceActors ABVA on ABVA.audioBooksId = ab.Id
LEFT JOIN VoiceActors VA on VA.voiceActorId = ABVA.voiceActorId
LEFT JOIN ProductTags PT on PT.productId = ab.Id AND PT.productTypeId = 2
LEFT JOIN Tags T on T.tagId = PT.tagId",
                ((book, author, voiceActor, tag) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        foundBook.VoiceActors = new List<VoiceActorDto>();
                        foundBook.Tags = new List<TagDto>();
                        cache.Add(foundBook);
                    }
                    if (author != null) {
                        if (foundBook.Authors.All(a => a.Id != author.Id))
                            foundBook.Authors.Add(author);
                    }

                    if (voiceActor != null) {
                        if (foundBook.VoiceActors.All(va => va.Id != voiceActor.Id))
                            foundBook.VoiceActors.Add(voiceActor);
                    }

                    if (tag != null) {
                        if (foundBook.Tags.All(t => t.Id != tag.Id))
                            foundBook.Tags.Add(tag);
                    }

                    return foundBook;
                }),
                new { accountId }, splitOn: "Id");
            return cache;
        }

        public async Task<List<RecommendedAudioBookDto>> SearchAudioBooksByRecommendations(RecommendationAudioBookCriteriaDto criteria) {
            var cache = new List<RecommendedAudioBookDto>();
            await GetConnection().QueryAsync<RecommendedAudioBookDto, AuthorDto, VoiceActorDto, TagDto, RecommendedAudioBookDto>(
                @"WITH AuthorScores AS (
    SELECT
        ab.audioBookId,
        COUNT(DISTINCT A.authorId) * 5 AS AuthorScore
    FROM AudioBooks ab
    JOIN AudioBooksAuthors BA
        ON BA.audioBookId = ab.audioBookId
    JOIN Authors A
        ON A.authorId = BA.authorId
    WHERE A.firstName IN @authors OR A.lastName IN @authors OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) IN @authors
    GROUP BY ab.audioBookId
),

VoiceActorScores AS (
    SELECT
        ab.audioBookId,
        COUNT(DISTINCT VA.voiceActorId) * 4 AS VoiceActorScore
    FROM AudioBooks ab
    JOIN AudioBooksVoiceActors ABVA
        ON ABVA.audioBooksId = ab.audioBookId
    JOIN VoiceActors VA
        ON VA.voiceActorId = ABVA.voiceActorId
    WHERE VA.firstName IN @voiceActors OR VA.lastName IN @voiceActors OR TRIM(CONCAT(VA.firstName, ' ', VA.lastName)) IN @voiceActors
    GROUP BY ab.audioBookId
),

TagScores AS (
    SELECT
        ab.audioBookId,
        COUNT(DISTINCT T.tagId) * 2 AS TagScore
    FROM AudioBooks ab
    JOIN ProductTags PT
        ON PT.productId = ab.audioBookId
        AND PT.productTypeId = 2
    JOIN Tags T
        ON T.tagId = PT.tagId
    WHERE T.name IN @tags
    GROUP BY ab.audioBookId
),

BookScores AS (
    SELECT
        ab.audioBookId,
        ab.name,

        ISNULL(a.AuthorScore, 0)
        + ISNULL(va.VoiceActorScore, 0)
        + ISNULL(t.TagScore, 0)
        AS Score

    FROM AudioBooks ab

    LEFT JOIN AuthorScores a
        ON a.audioBookId = ab.audioBookId

    LEFT JOIN VoiceActorScores va
        ON va.audioBookId = ab.audioBookId

    LEFT JOIN TagScores t
        ON t.audioBookId = ab.audioBookId

    WHERE
        ISNULL(a.AuthorScore, 0) > 0
        OR ISNULL(va.VoiceActorScore, 0) > 0
        OR ISNULL(t.TagScore, 0) > 0
),

TopBooks AS (
    SELECT TOP 10 *
    FROM BookScores
    ORDER BY Score DESC
)

SELECT
    TB.audioBookId AS Id,
    TB.name,
    TB.Score,

    A.authorId AS Id,
    A.firstName,
    A.lastName,

    VA.voiceActorId AS Id,
    VA.firstName,
    VA.lastName,

    T.tagId AS Id,
    T.name

FROM TopBooks TB

LEFT JOIN dbo.AudioBooksAuthors BA
    ON BA.audioBookId = TB.audioBookId

LEFT JOIN Authors A
    ON A.authorId = BA.authorId

LEFT JOIN AudioBooksVoiceActors ABVA
    ON ABVA.audioBooksId = TB.audioBookId

LEFT JOIN VoiceActors VA
    ON VA.voiceActorId = ABVA.voiceActorId

LEFT JOIN ProductTags PT
    ON PT.productId = TB.audioBookId
    AND PT.productTypeId = 2

LEFT JOIN Tags T
    ON T.tagId = PT.tagId

ORDER BY TB.Score DESC;
",
                ((book, author, voiceActor, tag) => {
                    var foundBook = cache.FirstOrDefault(b => b.Id == book.Id);
                    if (foundBook == null) {
                        foundBook = book;
                        foundBook.Authors = new List<AuthorDto>();
                        foundBook.VoiceActors = new List<VoiceActorDto>();
                        foundBook.Tags = new List<TagDto>();
                        cache.Add(foundBook);
                    }
                    if (author != null) {
                        if (foundBook.Authors.All(a => a.Id != author.Id))
                            foundBook.Authors.Add(author);
                    }

                    if (voiceActor != null) {
                        if (foundBook.VoiceActors.All(va => va.Id != voiceActor.Id))
                            foundBook.VoiceActors.Add(voiceActor);
                    }

                    if (tag != null) {
                        if (foundBook.Tags.All(t => t.Id != tag.Id))
                            foundBook.Tags.Add(tag);
                    }

                    return foundBook;
                }),
                new { authors = criteria.Authors, voiceActors = criteria.VoiceActors, tags = criteria.Tags }, splitOn: "Id");
            return cache;
        }

    }
}
