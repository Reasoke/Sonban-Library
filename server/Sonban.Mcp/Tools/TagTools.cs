using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Sonban.Common.Dto;

namespace Sonban.Mcp.Tools
{
    internal class TagTools : BaseTools
    {
        public TagTools(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : 
            base(httpClientFactory, httpContextAccessor)
        {
        }

        [McpServerTool(Name = "save_tags")]
        [Description("Saves tags to DB for any product (book or audiobook)."
        )]
        public async Task<CallToolResult> SaveTags(SaveBookTagsRequest request,
            CancellationToken cancellationToken = default)
        {
            return await GetSafeResult(async () =>
            {
                await PostAsync(
                    "/api/mcp/book/parse",
                    request,
                    cancellationToken
                );

                return new
                {
                    success = true,
                    productId = request.ProductId,
                    request.ProductType,
                    savedTags = request.Tags
                };
            });
        }
    }


}
