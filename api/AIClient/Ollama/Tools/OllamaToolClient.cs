using System.Runtime.CompilerServices;
using API.AIClient.Ollama.Tools;
using Google.Protobuf.WellKnownTypes;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace API.AIClient.Ollama
{
    public class OllamaToolClient : IAIClient
    {
        private readonly IOllamaApiClient ollamaApiClient;

        public OllamaToolClient(IOllamaApiClient apiClient)
        {
            ollamaApiClient = apiClient;
        }

        public async IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ollamaApiClient.SelectedModel = aIClientParam.Model;
            var chat = new Chat(ollamaApiClient, Prompts.Prompts.SystemPrompt);
            var stream = chat.SendAsync(aIClientParam.Prompt,
            tools:
            [
                new GetDateTimeTool(),
                new GetServerInfoTool(),
            ],
            cancellationToken: cancellationToken);
            await foreach (var chunk in stream)
            {
                yield return new AIClientResponse
                {
                    Content = chunk,
                    Done = false,
                };
            }

            yield return new AIClientResponse
            {
                Done = true
            };

        }
    }
}