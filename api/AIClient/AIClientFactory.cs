using System;
using Microsoft.Extensions.DependencyInjection;
using API.AIClient.Gemini;
using API.AIClient.Ollama;

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
            if (providerName != null && providerName.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                return serviceProvider.GetRequiredService<GoogleClient>();
            }
            return serviceProvider.GetRequiredService<OllamaClient>();
        }
    }
}