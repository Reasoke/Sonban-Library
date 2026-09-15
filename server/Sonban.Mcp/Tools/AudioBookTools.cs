using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Sonban.Api.Dto;
using Sonban.Common.Dto;

namespace Sonban.Mcp.Tools
{
    internal class AudioBookTools : BaseTools
    {
        public AudioBookTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
            base(httpClientFactory, httpContextAccessor)
        {
        }
        #region tools for parsing book for tags
        [McpServerTool(Name = "get_audiobook")]
        [Description("Analyzes a audiobook by productId." +
                     "Fetches audiobook title and description and returns raw book data for AI understanding." +
                     "Use as FIRST step before generating tags."
        )]
        public async Task<CallToolResult> AnalyzeAudioBookForTags(int productId,
            CancellationToken cancellationToken = default) {
            return await GetSafeResult(async () => {    
                var book = await GetAsync<AudioBookDetailsDto>(
                    $"/api/audiobooks/{productId}", cancellationToken
                );

                if (book == null || string.IsNullOrWhiteSpace(book.Description))
                    return new CallToolResult {
                        IsError = true,
                        Content = [new TextContentBlock { Text = "Book not found or has no description" }],
                    };

                return new {
                    productId,
                    productType = 2,
                    title = book.Name,
                    description = book.Description
                };
            });
        }

        [McpServerTool(Name = "generate_audiobook_tags")]
        [Description("Converts analyzed book information into semantic tags." +
                     "Extracts minimum 20 meaningful tags describing genres, themes, mood, atmosphere, psychology and story elements." +
                     "Tags should be English, lowercase and use underscores instead of spaces." +
                     "Use after analyze_audiobook_for_tags and before save_audiobook_tags."
        )]
        public SaveBookTagsRequest GenerateAudioBookTags(int productId, int productType, List<string> tags,
            CancellationToken cancellationToken = default) {
            return new SaveBookTagsRequest {
                ProductId = productId,
                ProductType = productType,
                Tags = tags
            };
        }
        #endregion

        #region tools for giving recomendations

        [McpServerTool(Name = "get_users_audiobooks")]
        [Description("Use this tool whenever recommendations should be personalized based on the user's existing audiobook library." +
            "Analyzes user's library (list of audiobooks with authors, voice actors and tags) and extracts stable reading preferences." +
            "Returns structured understanding of user's taste patterns." +
            "Use when you need to understand what user likes before making recommendations."
        )]
        public async Task<CallToolResult> AnalyzeAccountAudioBooks(CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                var books = await GetAsync<List<RecommendedAudioBookDto>>(
                    $"/api/mcp/recommend/account-library/get-audiobooks",
                    cancellationToken
                );

                if (books == null || books.Count == 0)
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = [new TextContentBlock {Text = "User has no books for analysis"}],
                    };

                return books;
            });
        }


        [McpServerTool(Name = "search_audiobook_by_criteria")]
        [Description("Searches candidate audiobooks." +
            "Returns candidate recommendations with score." +
            "Agent should select best audiobooks and explain relevance."
        )]
        public async Task<CallToolResult> GetRecommendations(RecommendationAudioBookCriteriaDto criteria,
            CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                var books = await PostAsync<List<RecommendedAudioBookDto>>(
                    "/api/mcp/recommend/audiobooks/search",
                    criteria,
                    cancellationToken
                );

                return books;
            });
        }

        #endregion
    }
}
