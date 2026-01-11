using API.AIClient.Gemini;
using API.AIClient.Ollama;
using Google.GenAI;

namespace API.AIClient
{
    public interface IAIClientFactory
    {
        IAIClient CreateClient(string providerName);
    }

    public class AIClientFactory : IAIClientFactory
    {
        private readonly IServiceProvider serviceProvider;
        public AIClientFactory(IServiceProvider provider)
        {
            serviceProvider = provider;
        }
        public IAIClient CreateClient(string providerName)
        {
            if (providerName.ToLower() == "google")
            {
                return serviceProvider.GetRequiredService<GoogleClient>();
            }
            if (providerName.ToLower() == "ollamatool")
            {
                return serviceProvider.GetRequiredService<OllamaToolClient>();
            }
            return serviceProvider.GetRequiredService<OllamaClient>();
        }
    }
}