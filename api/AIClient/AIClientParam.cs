namespace API.AIClient
{
    public class AIClientParam
    {
        public string Model { get; set; }
        public string Prompt { get; set; }

        public string Context { get; set; }

        public string StoreName { get; set; }

        public string SystemPrompt { get; set; }
    }
}