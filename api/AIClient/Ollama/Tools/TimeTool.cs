using OllamaSharp.Models.Chat;

namespace API.AIClient.Ollama.Tools
{
    public class TimeTool : IOllamaTool
    {
        public TimeTool()
        {
        }
        public string Name => "get_current_time";

        public string Description => "Use this tool to get the current date and time. It takes no input.";

        public Dictionary<string, Property> Parameters => [];

        public string[] Required => [];

        public string Action(Message.ToolCall toolCall)
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
        }
    }
}