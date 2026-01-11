using System.Runtime.CompilerServices;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace API.AIClient.Ollama
{
    public class OllamaClient : IAIClient
    {
        /// <summary>
        /// ollama API client
        /// </summary>
        private readonly IOllamaApiClient ollamaApi;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config"></param>
        public OllamaClient(IOllamaApiClient ollamaApiClient)
        {
            ollamaApi = ollamaApiClient;
        }
        /// <summary>
        /// <see cref="IAIClient.GenerateAsync(AIClientParam, CancellationToken)"/>
        /// </summary>
        /// <param name="aIClientParam"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (aIClientParam == null) { throw new ArgumentException("null Exception"); }
            ollamaApi.SelectedModel = aIClientParam.Model;
            var systemPrompt = string.IsNullOrEmpty(aIClientParam.SystemPrompt) ? Prompts.Prompts.SystemPrompt : aIClientParam.SystemPrompt;
            System.Console.WriteLine(systemPrompt);
            var messages = new List<Message>
            {
              new Message
              {
                  Role = "system",
                  Content = systemPrompt
              },
              new Message
              {
                  Role = "user",
                  Content = aIClientParam.Prompt
              }
            };
            var stream = ollamaApi.ChatAsync(new ChatRequest { Messages = messages, Stream = true }, cancellationToken);
            await foreach (var chunk in stream)
            {
                yield return new AIClientResponse
                {
                    Content = chunk.Message?.Content,
                    Done = chunk.Done
                };
            }
        }
    }
}