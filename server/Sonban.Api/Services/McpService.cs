using System.Net;
using Sonban.Api.Repository;
using Sonban.Api.Classes;
using Sonban.Common.Classes;
using Sonban.Common.Dto;

namespace Sonban.Api.Services
{

    public interface IMcpService {
        Task<bool> ParseBookForTags(SaveBookTagsRequest request);
        Task<List<RecommendedBookDto>> GetBooksFromUserLibraryForRecommendations(int userId);
        Task<List<RecommendedBookDto>> SearchBooksByRecommendations(RecommendationBookCriteriaDto criteria);
        Task<List<RecommendedAudioBookDto>> GetAudioBooksFromUserLibraryForRecommendations(int userId);
        Task<List<RecommendedAudioBookDto>> SearchAudioBooksByRecommendations(RecommendationAudioBookCriteriaDto criteria);
    }

    public class McpService : IMcpService {
        private readonly IMcpRepository mcpRepository;
        private readonly IAccountsRepository accountsRepository;

        public McpService(IMcpRepository mcpRepository, IAccountsRepository accountsRepository, ISettingsProvider settingsProvider) {
            this.mcpRepository = mcpRepository;
            this.accountsRepository = accountsRepository;
        }

        public async Task<bool> ParseBookForTags(SaveBookTagsRequest request) {
            var isTagged = await mcpRepository.IsBookTagged(request.ProductId, request.ProductType);
            if (isTagged) {
                throw new DomainException(HttpStatusCode.BadRequest, "Book is already tagged");
            }

            return await mcpRepository.ParseBookForTags(request);
        }

        public async Task<List<RecommendedBookDto>> GetBooksFromUserLibraryForRecommendations(int userId) {
            var account = await accountsRepository.GetAccount(userId);
            return await mcpRepository.GetBooksFromUserLibraryForRecommendations(account.Id);
        }

        public async Task<List<RecommendedBookDto>> SearchBooksByRecommendations(RecommendationBookCriteriaDto criteria) {
            return await mcpRepository.SearchBooksByRecommendations(criteria);
        }


        public async Task<List<RecommendedAudioBookDto>> GetAudioBooksFromUserLibraryForRecommendations(int userId) {
            var account = await accountsRepository.GetAccount(userId);
            return await mcpRepository.GetAudioBooksFromUserLibraryForRecommendations(account.Id);
        }

        public async Task<List<RecommendedAudioBookDto>> SearchAudioBooksByRecommendations(RecommendationAudioBookCriteriaDto criteria) {
            return await mcpRepository.SearchAudioBooksByRecommendations(criteria);
        }
    }
}
