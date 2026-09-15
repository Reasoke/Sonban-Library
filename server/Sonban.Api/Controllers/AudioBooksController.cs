using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;

namespace Sonban.Api.Controllers {
    [AuthCookieCheckFilter]
    public class AudioBooksController : BaseApiController {
        private readonly IAudioBooksService audiobooksService;
        private readonly IConfiguration _config;
        private readonly IAccountsService accountsService;

        public AudioBooksController(IAudioBooksService audiobooksService, IConfiguration config, IAccountsService accountsService) {
            this.audiobooksService = audiobooksService;
            _config = config;
            this.accountsService = accountsService;
        }

        [AllowAnonymous]
        [HttpGet("api/audiobooks")]
        public async Task<IEnumerable<AudioBookDto>> GetAudioBooks([FromQuery] string name, [FromQuery] string author, [FromQuery] string voiceActor, [FromQuery] int take = 20, [FromQuery] int skip = 0) {

            return await audiobooksService.GetAudioBooks(take, skip, name, author, voiceActor);
        }


        [HttpGet, Route("api/user/audiobooks")]
        public async Task<IEnumerable<AudioBookDto>> GetUsersAudioBooks([FromQuery] string name, [FromQuery] string author, [FromQuery] string voiceActor, [FromQuery] bool? isFavourite, [FromQuery] int take = 20, [FromQuery] int skip = 0) {
            var account = await accountsService.GetAccount(CurrentIdentity.UserId);

            var books = audiobooksService.GetUsersAudioBooks(take, skip, name, author, voiceActor, isFavourite, CurrentIdentity.UserId, account.Id);
            return await books;
        }

        [HttpGet, Route("api/{accountId:int}/audiobooks/favourite")]
        public async Task<IEnumerable<AudioBookDto>> GetFavouriteAudioBooks(int accountId, int take = 20, int skip = 0) {
            var audiobooks = audiobooksService.GetFavouriteAudioBooks(take, skip, CurrentIdentity.UserId, CurrentIdentity.UserEmail, accountId);
            return await audiobooks;
        }

        [AllowAnonymous]
        [HttpGet, Route("api/audiobooks/{audiobookId:int}")]
        public async Task<AudioBookDetailsDto> GetAudioBookDetails(int audiobookId) {
            var audiobook = await audiobooksService.GetAudioBookDetails(audiobookId);

            if (CurrentIdentity != null) {
                var account = await accountsService.GetAccount(CurrentIdentity.UserId);
                audiobook = await audiobooksService.GetAudioBookStatuses(audiobook, account.Id);
            }

            return audiobook;
        }

        [AllowAnonymous]
        [HttpGet("api/audiobooks/files/{audiobookId:int}/cover")]
        public async Task<IActionResult> GetBookCover(int audiobookId) {
            var filePath = await audiobooksService.GetAudioBookCover(audiobookId);

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
