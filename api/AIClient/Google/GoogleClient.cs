using System.Runtime.CompilerServices;
using Google.GenAI;

namespace API.AIClient.Gemini
{
    public class GoogleClient : IAIClient
    {

        private readonly Client googleClient;

        public GoogleClient(Client googleClient)
        {
            this.googleClient = googleClient;
        }

        public async IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (aIClientParam == null) { throw new ArgumentException("null exception"); }
            var stream = googleClient.Models.GenerateContentStreamAsync(
                model: "gemini-2.5-flash-lite",
                contents: aIClientParam.Prompt
            );

            await foreach (var chunk in stream)
            {
                yield return new AIClientResponse
                {
                    Content = chunk.Candidates[0].Content.Parts[0].Text,
                    Done = false
                };
            }

            yield return new AIClientResponse
            {
                Done = true
            };
        }

    }
}