using System;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OllamaSharp;
using API.Configuration;

namespace API.Rag.EmbeddingGenerator.Ollama
{
    public class OllamaEmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly OllamaApiClient apiClient;
        private readonly AiSettings aiSettings;

        public OllamaEmbeddingGenerator(IOllamaApiClient client, IOptions<AiSettings> options)
        {
            apiClient = (OllamaApiClient)client;
            aiSettings = options.Value;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            apiClient.SelectedModel = string.IsNullOrEmpty(aiSettings.Ollama.EmbeddingModel) ? "nomic-embed-text" : aiSettings.Ollama.EmbeddingModel;
            var result = await apiClient.GenerateVectorAsync(text).ConfigureAwait(false);
            return result.ToArray();
        }
    }
}