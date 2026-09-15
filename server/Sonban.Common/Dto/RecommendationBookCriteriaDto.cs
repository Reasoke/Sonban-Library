using System.Collections.Generic;

namespace Sonban.Common.Dto {
    public class RecommendationBookCriteriaDto {
        public List<string> Genres { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public List<string> Authors { get; set; } = new();
    }
    
    public class RecommendationAudioBookCriteriaDto {
        public List<string> VoiceActors { get; set; } = new();
        public List<string> Authors { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class SaveBookTagsRequest {
        public int ProductId { get; set; }
        public int ProductType { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}

