using System.Collections.Generic;

namespace API.Configuration
{
    public class AiSettings
    {
        public string DefaultClient { get; set; } = "google";
        public string DefaultModel { get; set; } = "gemini-2.5-flash-lite";
        public string DefaultStoreName { get; set; } = "personal";
        public string DefaultSystemPrompt { get; set; } = "";
        public List<string> AvailableClients { get; set; } = new();
        public Dictionary<string, List<string>> Models { get; set; } = new();

        public OllamaSettings Ollama { get; set; } = new();
        public GoogleSettings Google { get; set; } = new();
    }

    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string DefaultModel { get; set; } = "llama3.1:8b";
        public string EmbeddingModel { get; set; } = "nomic-embed-text";
        public int EmbeddingDimension { get; set; } = 768;
    }

    public class GoogleSettings
    {
        public string ApiKey { get; set; } = "";
        public string DefaultModel { get; set; } = "gemini-2.5-flash-lite";
    }
}
