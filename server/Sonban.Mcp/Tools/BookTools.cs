using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Sonban.Common.Dto;

namespace Sonban.Mcp.Tools
{
    internal class BookTools : BaseTools
    {
        public BookTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
            base(httpClientFactory, httpContextAccessor)
        {
        }

        #region tools for parsing book for tags
        [McpServerTool(Name = "get_book")]
        [Description("Analyzes a book by productId." +
                     "Fetches book title and description and returns raw book data for AI understanding." +
                     "Use as FIRST step before generating tags."
        )]
        public async Task<CallToolResult> AnalyzeBookForTags(int productId,
            CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                var book = await GetAsync<BookDetailsDto>(
                    $"/api/books/{productId}", cancellationToken
                );

                if (book == null || string.IsNullOrWhiteSpace(book.Description))
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = [new TextContentBlock {Text = "Book not found or has no description"}],
                    };

                return new
                {
                    productId,
                    productType = 1,
                    title = book.Name,
                    description = book.Description
                };
            });
        }

        #endregion

        #region tools for giving recomendations

        [McpServerTool(Name = "get_users_books")]
        [Description("Use this tool whenever recommendations should be personalized based on the user's existing book library." + 
            "Analyzes user's library (list of books with authors, genres and tags) and extracts stable reading preferences. " +
            "Returns structured understanding of user's taste patterns. " +
            "Use when you need to understand what user likes before making recommendations."
        )]
        public async Task<CallToolResult> AnalyzeAccountBooks(CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                var books = await GetAsync<List<RecommendedBookDto>>(
                    $"/api/mcp/recommend/account-library/get-books",
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

        [McpServerTool(Name = "search_books_by_criteria")]
        [Description("Searches candidate books." +
            "Returns candidate recommendations with score." +
            "Agent should select best books and explain relevance."
        )]
        public async Task<CallToolResult> GetRecommendations(RecommendationBookCriteriaDto criteria,
            CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                var books = await PostAsync<List<RecommendedBookDto>>(
                    "/api/mcp/recommend/books/search",
                    criteria,
                    cancellationToken
                );

                return books;
                
            });
        }
        #endregion
    }

}
