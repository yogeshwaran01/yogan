using System.Collections.Generic;
using API.History;
using Microsoft.AspNetCore.Mvc;

namespace API.AIClient
{
    public class AIClientParam
    {
        public string Client { get; set; }
        public string Model { get; set; }
        public string SystemPrompt { get; set; }
        public string Prompt { get; set; }
        public string Context { get; set; }
        public string StoreName { get; set; }
        public IFormFile FormFile { get; set; }
        public string ConversationId { get; set; }
        public List<ChatMessage> History { get; set; } = new();

        public bool IsRagEnabled { get; set; } = true;

        public bool IsValidForGenerate()
        {
            if (IsEmpty(Prompt)) { return false; }
            return true;
        }

        public bool IsValidForContext()
        {
            if (IsEmpty(Context)) { return false; }
            return true;
        }

        public bool IsValidForFileContext()
        {
            if (FormFile == null) { return false; }
            return true;
        }

        private static bool IsEmpty(string content)
        {
            return string.IsNullOrWhiteSpace(content);
        }
    }
}