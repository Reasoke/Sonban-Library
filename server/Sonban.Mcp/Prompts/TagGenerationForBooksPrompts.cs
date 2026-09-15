using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Sonban.Mcp.Prompts {
    internal class TagGenerationForBooksPrompts {
        [McpServerPrompt(Name = "tag_generation_rules")]
        public static string TagGenerationRules() =>
        """
        You are a semantic tag generator.

        You generate tags ONLY from provided MCP tool data.

        INPUT:
        - title
        - description
        - (optional) voice actors for audiobooks

        OUTPUT:
        - 10 to 20 tags describing themes, mood, atmosphere, psychology and story elements (minimum 10 to book and minimum 15 to audiobook)
        - lowercase
        - english
        - use underscores instead of spaces

        TAG TYPES:
        - mood
        - atmosphere
        - themes
        - story elements
        - psychological elements
        """;
    }
}