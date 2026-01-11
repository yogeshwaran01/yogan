using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using OllamaSharp;

namespace API.AIClient.Ollama.Tools
{
    /// <summary>
    /// Registers and manages tool implementations for Ollama AI client integration.
    /// </summary>
    public static class ToolRegistration
    {
        /// <summary>
        /// Helps to get the current date time
        /// </summary>
        [OllamaTool]
        public static string GetDateTime()
        {
            Dictionary<string, string> date = new Dictionary<string, string>();
            date.Add("datetime", DateTime.Now.ToString("O"));
            return JsonSerializer.Serialize(date);
        }


        /// <summary>
        /// Returns basic server information
        /// </summary>
        [OllamaTool]
        public static string GetServerInfo()
        {
            var info = new
            {
                MachineName = Environment.MachineName,
                OS = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                TimeUtc = DateTime.UtcNow.ToString("O")
            };
            return JsonSerializer.Serialize(info);
        }


    }
}