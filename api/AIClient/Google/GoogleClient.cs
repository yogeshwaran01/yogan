using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Options;
using Google.GenAI;
using Google.GenAI.Types;
using API.Configuration;

namespace API.AIClient.Gemini
{
    public class GoogleClient : IAIClient
    {
        private readonly Client googleClient;
        private readonly AiSettings aiSettings;

        public GoogleClient(Client googleClient, IOptions<AiSettings> options)
        {
            this.googleClient = googleClient;
            aiSettings = options.Value;
        }

        public async IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (aIClientParam == null) { throw new ArgumentNullException(nameof(aIClientParam)); }

            var modelName = string.IsNullOrEmpty(aIClientParam.Model) ? aiSettings.Google.DefaultModel : aIClientParam.Model;
            var systemPrompt = string.IsNullOrEmpty(aIClientParam.SystemPrompt) ? aiSettings.DefaultSystemPrompt : aIClientParam.SystemPrompt;

            var config = new GenerateContentConfig();
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                config.SystemInstruction = new Content
                {
                    Parts = new List<Part> { new Part { Text = systemPrompt } }
                };
            }

            var contents = new List<Content>();

            // Append history
            if (aIClientParam.History != null)
            {
                foreach (var msg in aIClientParam.History)
                {
                    contents.Add(new Content
                    {
                        Role = msg.Role.ToLower() == "assistant" ? "model" : "user",
                        Parts = new List<Part> { new Part { Text = msg.Content } }
                    });
                }
            }

            // Append current prompt
            contents.Add(new Content
            {
                Role = "user",
                Parts = new List<Part> { new Part { Text = aIClientParam.Prompt } }
            });

            var stream = googleClient.Models.GenerateContentStreamAsync(
                model: modelName,
                contents: contents,
                config: config
            );

            await foreach (var chunk in stream)
            {
                if (chunk.Candidates != null && chunk.Candidates.Count > 0 &&
                    chunk.Candidates[0].Content != null && chunk.Candidates[0].Content.Parts != null &&
                    chunk.Candidates[0].Content.Parts.Count > 0)
                {
                    yield return new AIClientResponse
                    {
                        Content = chunk.Candidates[0].Content.Parts[0].Text,
                        Done = false
                    };
                }
            }

            yield return new AIClientResponse
            {
                Done = true
            };
        }
    }
}