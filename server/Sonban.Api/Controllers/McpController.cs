using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Services;
using Sonban.Common.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Controllers {
    
    [AuthMcpTokenCheck]
    public class McpController : BaseApiController {
        private readonly IMcpService mcpService;

        public McpController(IMcpService mcpService) {
            this.mcpService = mcpService;
        }

        [CheckSysAdminRights] //check
        [HttpPost("api/mcp/book/parse")]
        public async Task<IActionResult> ParseBookForTags([FromBody] SaveBookTagsRequest request) {
            var res = await mcpService.ParseBookForTags(request);
            if (!res) {
                throw new DomainException(HttpStatusCode.NotFound, "Book is not found in database");
            }
            return Ok();
        }


        [HttpGet("api/mcp/recommend/account-library/get-books")]
        public async Task<List<RecommendedBookDto>> GetBooksFromUserLibraryForRecommendations() {
            return await mcpService.GetBooksFromUserLibraryForRecommendations(CurrentIdentity.UserId);
        }

        [HttpPost("api/mcp/recommend/books/search")]
        public async Task<List<RecommendedBookDto>> SearchBooksByRecommendations([FromBody] RecommendationBookCriteriaDto criteria) {
            return await mcpService.SearchBooksByRecommendations(criteria);
        }


        [HttpGet("api/mcp/recommend/account-library/get-audiobooks")]
        public async Task<List<RecommendedAudioBookDto>> GetAudioBooksFromUserLibraryForRecommendations() {
            return await mcpService.GetAudioBooksFromUserLibraryForRecommendations(CurrentIdentity.UserId);
        }

        [HttpPost("api/mcp/recommend/audiobooks/search")]
        public async Task<List<RecommendedAudioBookDto>> SearchAudioBooksByRecommendations([FromBody] RecommendationAudioBookCriteriaDto criteria) {
            return await mcpService.SearchAudioBooksByRecommendations(criteria);
        }
    }
}
