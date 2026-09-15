using System.Net;
using Sonban.Api.Dto;
using Sonban.Api.Repository;
using Sonban.Api.Classes;
using Sonban.Common.Classes;

namespace Sonban.Api.Services
{
    public interface IAudioBooksService {
        Task<IEnumerable<AudioBookDto>> GetAudioBooks(int take, int skip, string name, string author, string voiceActor);
        Task<IEnumerable<AudioBookDto>> GetUsersAudioBooks(int take, int skip, string name, string author, string voiceActor, bool? isFavourite, int userId, int accountId);
        Task<IEnumerable<AudioBookDto>> GetFavouriteAudioBooks(int take, int skip, int userId, string email, int accountId);
        Task<AudioBookDetailsDto> GetAudioBookDetails(int audiobookId);
        Task<AudioBookDetailsDto> GetAudioBookStatuses(AudioBookDetailsDto audiobook, int accountId);
        Task<string> GetAudioBookCover(int audiobookId);

        //Task<string> GetBookFilePath(int bookId, int accountId, string email);
        //Task<int> UploadBook(Stream stream);
        //Task<string> GetBookCover(int bookId);
    }

    public class AudioBooksService : IAudioBooksService {
        private readonly IAudioBooksRepository audiobooksRepository;
        private readonly IAccountsRepository accountsRepository;
        private readonly string audioBooksPath;
        private readonly string defaultAudioBookCoverPath;

        public AudioBooksService(IAudioBooksRepository audiobooksRepository, IAccountsRepository accountsRepository, ISettingsProvider settingsProvider) {
            this.audiobooksRepository = audiobooksRepository;
            this.accountsRepository = accountsRepository;
            audioBooksPath = settingsProvider.GetValue<string>("AudioBooksPath");
            defaultAudioBookCoverPath = settingsProvider.GetValue<string>("DefaultAudioBookCoverPath");
        }

        public async Task<IEnumerable<AudioBookDto>> GetAudioBooks(int take, int skip, string name, string author, string voiceActor) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            return await audiobooksRepository.GetAudioBooks(take, skip, name, author, voiceActor);
        }

        public async Task<IEnumerable<AudioBookDto>> GetUsersAudioBooks(int take, int skip, string name, string author, string voiceActor, bool? isFavourite, int userId, int accountId) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            return await audiobooksRepository.GetUsersAudioBooks(take, skip, name, author, voiceActor, accountId, isFavourite);
        }

        public async Task<IEnumerable<AudioBookDto>> GetFavouriteAudioBooks(int take, int skip, int userId, string email, int accountId) {
            if (take < 1)
                throw new ArgumentException("Take is too small, 1 is a min.");
            if (take > 1000)
                throw new ArgumentException("Take is too big, 100 is a max.");

            var registered = await accountsRepository.IsUserInAccount(accountId, email);
            if (!registered) {
                throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
            }

            return await audiobooksRepository.GetFavouriteAudioBooks(take, skip, accountId);
        }

        public async Task<AudioBookDetailsDto> GetAudioBookDetails(int audiobookId) {
            return await audiobooksRepository.GetAudioBookDetails(audiobookId);
        }
        
        public async Task<AudioBookDetailsDto> GetAudioBookStatuses(AudioBookDetailsDto audiobook, int accountId) {
            return await audiobooksRepository.GetAudioBookStatuses(audiobook, accountId);
        }


        public async Task<string> GetAudioBookCover(int audiobookId) {
            var fileName = await audiobooksRepository.GetAudioBookFileName(audiobookId);
            //if (fileName != null) {
            //    var coverPath = Path.GetFileNameWithoutExtension(fileName);
            //    coverPath = Path.Combine(audioBooksPath, coverPath, "images");
            //    //if (!Directory.Exists(coverPath)) {
            //    //    var process = Process.Start(new ProcessStartInfo(bookConverterPath, $"{fileName} -epub -preview") {
            //    //        //UseShellExecute = true,
            //    //        CreateNoWindow = true
            //    //    });
            //    //    process.WaitForExit();
            //    //}

            //    var files = Directory.GetFiles(coverPath, "cover*.*");
            //    if (files.Length > 0) {
            //        return files[0];
            //    }
            //    files = Directory.GetFiles(coverPath, "*.*");
            //    if (files.Length > 0) {
            //        return files[0];
            //    }
            //}
            return defaultAudioBookCoverPath;
        }


        //public async Task SetRating(int bookId, int accountId, decimal rating, string email) {
        //    var registered = await accountsRepository.IsUserInAccount(accountId, email);
        //    if (!registered) {
        //        throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
        //    }

        //    await audiobooksRepository.SetRating(bookId, accountId, rating);
        //}


        //public async Task<string> GetBookFilePath(int bookId, int accountId, string email) {
        //    var registered = await accountsRepository.IsUserInAccount(accountId, email);
        //    if (!registered) {
        //        throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
        //    }
        //    var bookInAcc = await audiobooksRepository.IsBookInAccount(bookId, accountId);
        //    if (!bookInAcc) {
        //        throw new DomainException(HttpStatusCode.BadRequest, "This book is not bought");
        //    }

        //    var fileName = await audiobooksRepository.GetBookFileName(bookId);
        //    return Path.Combine(booksPath, fileName);
        //}

        //public async Task<string> GetBookCover(int bookId) {
        //    var fileName = await audiobooksRepository.GetBookFileName(bookId);
        //    if (fileName != null) {
        //        var coverPath = Path.GetFileNameWithoutExtension(fileName);
        //        coverPath = Path.Combine(booksPath, coverPath, "images");
        //        if (!Directory.Exists(coverPath)) {
        //            var process = Process.Start(new ProcessStartInfo(bookConverterPath, $"{fileName} -epub -preview") {
        //                //UseShellExecute = true,
        //                CreateNoWindow = true
        //            });
        //            process.WaitForExit();
        //        }

        //        var files = Directory.GetFiles(coverPath, "cover*.*");
        //        if(files.Length > 0) {
        //            return files[0];
        //        }
        //        files = Directory.GetFiles(coverPath, "*.*");
        //        if (files.Length > 0) {
        //            return files[0];
        //        }
        //    }
        //    return defaultBookCoverPath;
        //}

        //public async Task<int> UploadBook(Stream inputStream) {
        //    var uniqueFileName = Guid.NewGuid().ToString() + ".fb2";
        //    var filePath = Path.Combine(booksPath, uniqueFileName);

        //    using (var stream = new FileStream(filePath, FileMode.Create)) {
        //        await inputStream.CopyToAsync(stream);
        //    }

        //    var bookDetails = BookHelper.GetBookDetails(filePath);

        //    await audiobooksRepository.AddNewBook(bookDetails, uniqueFileName);

        //    return bookDetails.Id;
        //}


        //public async Task AddBookToAccount(int bookId, int accountId, string email) {
        //    var registered = await accountsRepository.IsUserInAccount(accountId, email);
        //    if (!registered) {
        //        throw new DomainException(HttpStatusCode.BadRequest, "User has no access to this account");
        //    }

        //    await audiobooksRepository.AddBookToAccount(bookId, accountId);
        //}
    }
}
