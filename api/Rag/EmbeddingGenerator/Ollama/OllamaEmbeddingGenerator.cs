using Microsoft.Extensions.AI;
using OllamaSharp;

namespace API.Rag.EmbeddingGenerator.Ollama
{

    public class OllamaEmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly OllamaApiClient apiClient;

        public OllamaEmbeddingGenerator(IOllamaApiClient client)
        {
            apiClient = (OllamaApiClient)client;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            apiClient.SelectedModel = "nomic-embed-text";
            var result = await apiClient.GenerateVectorAsync(text).ConfigureAwait(false);
            return result.ToArray();
        }
    }
}