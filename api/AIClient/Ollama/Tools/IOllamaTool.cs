using Microsoft.Extensions.AI;
using OllamaSharp.Models.Chat;
using static OllamaSharp.Models.Chat.Message;

namespace API.AIClient.Ollama.Tools
{
    public interface IOllamaTool
    {
        public Dictionary<string, Property> Parameters { get; }
        public string Name { get; }
        public string Description { get; }
        public string Action(ToolCall toolCall);

        public string[] Required { get; }
    }
}