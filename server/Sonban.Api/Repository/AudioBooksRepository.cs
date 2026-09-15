using Dapper;
using Sonban.Api.Dto;
using Sonban.Api.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Repository
{
    public interface IAudioBooksRepository {
        Task<IEnumerable<AudioBookDto>> GetAudioBooks(int take, int skip, string name, string author, string voiceActor);
        Task<IEnumerable<AudioBookDto>> GetUsersAudioBooks(int take, int skip, string name, string author, string voiceActor, int accountId, bool? isFavourite);
        Task<IEnumerable<AudioBookDto>> GetFavouriteAudioBooks(int take, int skip, int accountId);
        Task<AudioBookDetailsDto> GetAudioBookDetails(int audiobookId);
        Task<AudioBookDetailsDto> GetAudioBookStatuses(AudioBookDetailsDto audiobook, int accountId);
        Task<string> GetAudioBookFileName(int audiobookId);
    }

    public class AudioBooksRepository : BaseRepository, IAudioBooksRepository {
        public AudioBooksRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
        }

        public async Task<IEnumerable<AudioBookDto>> GetAudioBooks(int take, int skip, string name, string author, string voiceActor) {
            var cache = new List<AudioBookDto>();

            var query = @"WITH cte(audioBookId, name, price) AS (
    SELECT ab.audioBookId as id, ab.name, ab.price FROM AudioBooks ab WITH (NOLOCK)
        LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON ab.audioBookId = BA.audioBookId
        LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
        LEFT JOIN AudioBooksVoiceActors BV WITH (NOLOCK) ON ab.audioBookId = BV.audioBooksId
        LEFT JOIN VoiceActors V WITH (NOLOCK) ON BV.voiceActorId = V.voiceActorId
    WHERE (@name IS NULL OR ab.name LIKE @name)
    AND (@author IS NULL OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) LIKE @author)
    AND (@voiceActor IS NULL OR TRIM(CONCAT(V.firstName, ' ', V.lastName)) LIKE @voiceActor)
    GROUP BY ab.audioBookId, ab.name, ab.price
    ORDER BY ab.name
    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
)

SELECT b.audioBookId as id, b.name, b.price, A.authorId as Id, A.lastName as lastName, A.firstName as firstName,  V.voiceActorId as Id, V.lastName as lastName, V.firstName as firstName
FROM cte b WITH (NOLOCK)
LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON b.audioBookId = BA.audioBookId
LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
LEFT JOIN AudioBooksVoiceActors BV WITH (NOLOCK) ON b.audioBookId = BV.audioBooksId
LEFT JOIN VoiceActors V WITH (NOLOCK) ON BV.voiceActorId = V.voiceActorId";

            using var cn = GetConnection();
            await cn.QueryAsync<AudioBookDto, AuthorDto, VoiceActorDto, AudioBookDto>(
                query,
                (audiobook, authorObj, voiceActorObj) => {
                    var foundAudioBook = cache.FirstOrDefault(b => b.Id == audiobook.Id);

                    if (foundAudioBook == null) {
                        foundAudioBook = audiobook;
                        foundAudioBook.Authors = new List<AuthorDto>();
                        foundAudioBook.VoiceActors = new List<VoiceActorDto>();
                        cache.Add(foundAudioBook);
                    }

                    if (authorObj != null && foundAudioBook.Authors.All(a => a.Id != authorObj.Id)) {
                        foundAudioBook.Authors.Add(authorObj);
                    }

                    if (voiceActorObj != null && foundAudioBook.VoiceActors.All(a => a.Id != voiceActorObj.Id)) {
                        foundAudioBook.VoiceActors.Add(voiceActorObj);
                    }

                    return foundAudioBook;
                },
                new {
                    skip,
                    take,
                    name = name != null ? $"%{name}%" : null,
                    author = author != null ? $"%{author}%" : null,
                    voiceActor = voiceActor != null ? $"%{voiceActor}%" :null
                },
                splitOn: "Id, Id"
            );

            return cache;
        }

        public async Task<IEnumerable<AudioBookDto>> GetUsersAudioBooks(int take, int skip, string name, string author, string voiceActor, int accountId, bool? isFavourite) {
            var cache = new List<AudioBookDto>();

            var query = @"WITH cte(audioBookId, name, price) AS (
    SELECT ab.audioBookId as id, ab.name, ab.price FROM AudioBooks ab WITH (NOLOCK)
        JOIN AccountBooks UB on ab.audioBookId = UB.objectId and UB.typeId = 2 and UB.accountId=@accountId
        LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON ab.audioBookId = BA.audioBookId
        LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
        LEFT JOIN AudioBooksVoiceActors BV WITH (NOLOCK) ON ab.audioBookId = BV.audioBooksId
        LEFT JOIN VoiceActors V WITH (NOLOCK) ON BV.voiceActorId = V.voiceActorId
    WHERE (@name IS NULL OR ab.name LIKE @name)
    AND (@author IS NULL OR TRIM(CONCAT(A.firstName, ' ', A.lastName)) LIKE @author)
    AND (@voiceActor IS NULL OR TRIM(CONCAT(V.firstName, ' ', V.lastName)) LIKE @voiceActor)
    AND (@isFavourite IS NULL OR UB.isFavourite = @isFavourite)
    GROUP BY ab.audioBookId, ab.name, ab.price
    ORDER BY ab.name
    OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
)

SELECT b.audioBookId as id, b.name, b.price, A.authorId as Id, A.lastName as lastName, A.firstName as firstName,  V.voiceActorId as Id, V.lastName as lastName, V.firstName as firstName
FROM cte b WITH (NOLOCK)
LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON b.audioBookId = BA.audioBookId
LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
LEFT JOIN AudioBooksVoiceActors BV WITH (NOLOCK) ON b.audioBookId = BV.audioBooksId
LEFT JOIN VoiceActors V WITH (NOLOCK) ON BV.voiceActorId = V.voiceActorId";

            using var cn = GetConnection();
            await cn.QueryAsync<AudioBookDto, AuthorDto, VoiceActorDto, AudioBookDto>(
                query,
                (audiobook, authorObj, voiceActorObj) => {
                    var foundAudioBook = cache.FirstOrDefault(b => b.Id == audiobook.Id);

                    if (foundAudioBook == null) {
                        foundAudioBook = audiobook;
                        foundAudioBook.Authors = new List<AuthorDto>();
                        foundAudioBook.VoiceActors = new List<VoiceActorDto>();
                        cache.Add(foundAudioBook);
                    }

                    if (authorObj != null && foundAudioBook.Authors.All(a => a.Id != authorObj.Id)) {
                        foundAudioBook.Authors.Add(authorObj);
                    }

                    if (voiceActorObj != null && foundAudioBook.VoiceActors.All(a => a.Id != voiceActorObj.Id)) {
                        foundAudioBook.VoiceActors.Add(voiceActorObj);
                    }

                    return foundAudioBook;
                },
                new {
                    skip,
                    take,
                    accountId,
                    name = name != null ? $"%{name}%" : null,
                    author = author != null ? $"%{author}%" : null,
                    voiceActor = voiceActor != null ? $"%{voiceActor}%" : null,
                    isFavourite
                },
                splitOn: "Id, Id"
            );

            return cache;
        }

        public async Task<IEnumerable<AudioBookDto>> GetFavouriteAudioBooks(int take, int skip, int accountId) {//
            var cache = new List<AudioBookDto>();

            var query = @"with cte(audioBookId, name, price) as (
    SELECT ab.audioBookId as id, ab.name, ab.price FROM AudioBooks ab WITH (NOLOCK)
ORDER BY ab.audioBookId
OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY)

SELECT b.audioBookId as id, b.name, b.price,
       A.authorId as Id, A.lastName as lastName, A.firstName as firstName
FROM cte b
LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON b.audioBookId = BA.audioBookId
LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
JOIN AccountBooks UB on b.audioBookId = UB.objectId and UB.typeId = 2 and UB.accountId=@accountId and UB.isFavourite = 1";

            using var cn = GetConnection();
            await cn.QueryAsync<AudioBookDto, AuthorDto, AudioBookDto>(
                query,
                (audiobook, authorObj) => {
                    var foundAudioBook = cache.FirstOrDefault(b => b.Id == audiobook.Id);

                    if (foundAudioBook == null) {
                        foundAudioBook = audiobook;
                        foundAudioBook.Authors = new List<AuthorDto>();
                        cache.Add(foundAudioBook);
                    }

                    if (authorObj != null && foundAudioBook.Authors.All(a => a.Id != authorObj.Id)) {
                        foundAudioBook.Authors.Add(authorObj);
                    }

                    return foundAudioBook;
                },
                new {
                    skip,
                    take,
                    accountId
                },
                splitOn: "Id"
            );

            return cache;
        }

        public async Task<AudioBookDetailsDto> GetAudioBookDetails(int audiobookId) {
            var cache = new List<AudioBookDetailsDto>();

            await GetConnection().QueryAsync<AudioBookDetailsDto, AuthorDto, VoiceActorDto, AudioBookDetailsDto>(
                @"SELECT ab.audioBookId as id, ab.name, ab.description,ab.createdYear, ab.price,
        ROUND(AVG(UB.rating), 1) as rating,
       A.authorId as Id, A.lastName as lastName, A.firstName as firstName,
       V.voiceActorId as Id, V.lastName as lastName, V.firstName as firstName
FROM AudioBooks ab
LEFT JOIN AudioBooksAuthors BA WITH (NOLOCK) ON ab.audioBookId = BA.audioBookId
LEFT JOIN Authors A WITH (NOLOCK) ON A.authorId = BA.authorId
LEFT JOIN AudioBooksVoiceActors BV WITH (NOLOCK) ON ab.audioBookId = BV.audioBooksId
LEFT JOIN VoiceActors V WITH (NOLOCK) ON BV.voiceActorId = V.voiceActorId
LEFT JOIN AccountBooks UB ON ab.audioBookId = UB.objectId and UB.typeId = 2
WHERE ab.audioBookId = @audioBookId
group by ab.audioBookId, ab.name, ab.description, ab.createdYear, ab.price, A.authorId, A.lastName, A.firstName, V.voiceActorId, V.lastName, V.firstName",
                (audiobook, authorObj, voiceActorObj) => {
                    var foundAudioBook = cache.FirstOrDefault(b => b.Id == audiobook.Id);

                    if (foundAudioBook == null) {
                        foundAudioBook = audiobook;
                        foundAudioBook.Authors = new List<AuthorDto>();
                        foundAudioBook.VoiceActors = new List<VoiceActorDto>();
                        cache.Add(foundAudioBook);
                    }

                    if (authorObj != null && foundAudioBook.Authors.All(a => a.Id != authorObj.Id)) {
                        foundAudioBook.Authors.Add(authorObj);
                    }

                    if (voiceActorObj != null && foundAudioBook.VoiceActors.All(a => a.Id != voiceActorObj.Id)) {
                        foundAudioBook.VoiceActors.Add(voiceActorObj);
                    }

                    foundAudioBook.isFavourite = false;
                    foundAudioBook.isBought = false;

                    return foundAudioBook;
                },
                new { audiobookId },
                splitOn: "Id, Id"
            );
            return cache.FirstOrDefault();
        
        }        
        
        public async Task<AudioBookDetailsDto> GetAudioBookStatuses(AudioBookDetailsDto audiobook, int accountId) {
            var res = await GetConnection().QueryFirstOrDefaultAsync(
                @"SELECT isFavourite, rating FROM AccountBooks where typeId = 2 and accountId = @accountId and objectId = @audiobookId",
                new { audiobookId = audiobook.Id, accountId });

            if (res != null) {
                audiobook.isBought = true;
                audiobook.isFavourite = res.isFavourite;
                audiobook.personalRating = res.rating ?? 0;
            }
            return audiobook;
        }

        public async Task<string> GetAudioBookFileName(int audiobookId) {
            return await GetConnection().QueryFirstOrDefaultAsync<string>(
            @"SELECT path FROM Files WHERE typeId = 2 and objectId = @audiobookId",
            new { audiobookId });
        }

          
    }
}
