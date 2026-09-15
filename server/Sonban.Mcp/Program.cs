using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sonban.Mcp.Classes;
using Sonban.Mcp.Prompts;
using Sonban.Mcp.Tools;

namespace Sonban.Mcp {
    internal class Program {
        static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            ConfigureServices(builder.Services, builder.Configuration);
         
            builder.Services.AddHttpClient("LibraryApi", client => {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("apiServiceUrl"));
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            builder.Services.AddHttpContextAccessor();
            
            builder.Services.AddMcpServer()
                .WithHttpTransport()
                .WithTools<BookTools>()
                .WithTools<AudioBookTools>()
                .WithTools<TagTools>()
                .WithPrompts<GeneralRecommendationPrompts>()
                .WithPrompts<TagGenerationForBooksPrompts>()
            ;

            var app = builder.Build();
            
            app.UseMiddleware<McpTokenAuthMiddleware>();
            
            app.MapMcp("/mcp");
            
            var appServiceUrl = builder.Configuration.GetValue<string>("appServiceUrl");
            await app.RunAsync(appServiceUrl);
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //todo:
        }
    }
}
