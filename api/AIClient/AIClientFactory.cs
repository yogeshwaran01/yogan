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
            return serviceProvider.GetRequiredService<OllamaClient>();
        }
    }
}