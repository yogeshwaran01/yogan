using Microsoft.AspNetCore.Mvc;
using Qdrant.Client.Grpc;

namespace API.AIClient
{
    public class AIClientParam
    {
        public string Client { get; set; }
        public string Model { get; set; }
        public string Prompt { get; set; }
        public string Context { get; set; }
        public string StoreName { get; set; }
        public IFormFile FormFile { get; set; }

        public bool IsRagEnabled { get; set; } = true;

        public void AddDefaults()
        {
            if (IsEmpty(Client)) { Client = "ollama"; }
            if (IsEmpty(Model)) { Model = "llama3.1:8b"; }
            if (IsEmpty(StoreName)) { StoreName = "store"; }
        }

        public bool IsValidForGenerate()
        {
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(StoreName)) { return false; }
            if (IsEmpty(Prompt)) { return false; }
            return true;
        }

        public bool IsValidForContext()
        {
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(StoreName)) { return false; }
            if (IsEmpty(Prompt)) { return false; }
            if (IsEmpty(Context)) { return false; }
            return true;
        }

        public bool IsValidForFileContext()
        {
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(Model)) { return false; }
            if (IsEmpty(StoreName)) { return false; }
            if (IsEmpty(Prompt)) { return false; }
            if (FormFile == null) { return false; }
            return true;

        }

        private static bool IsEmpty(string content)
        {
            if (content == null) { return true; }
            if (string.IsNullOrEmpty(content)) { return true; }
            return false;
        }
    }
}