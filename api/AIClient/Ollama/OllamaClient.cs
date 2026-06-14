using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharp.Tools;
using API.Configuration;
using API.AIClient.Ollama.Tools;

namespace API.AIClient.Ollama
{
    public class OllamaClient : IAIClient
    {
        private readonly IOllamaApiClient ollamaApi;
        private readonly OllamaToolsRegistry toolsRegistry;
        private readonly AiSettings aiSettings;

        public OllamaClient(IOllamaApiClient ollamaApiClient, OllamaToolsRegistry registry, IOptions<AiSettings> options)
        {
            ollamaApi = ollamaApiClient;
            toolsRegistry = registry;
            aiSettings = options.Value;
        }

        public async IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (aIClientParam == null) { throw new ArgumentNullException(nameof(aIClientParam)); }

            ollamaApi.SelectedModel = string.IsNullOrEmpty(aIClientParam.Model) ? aiSettings.Ollama.DefaultModel : aIClientParam.Model;
            var systemPrompt = string.IsNullOrEmpty(aIClientParam.SystemPrompt) ? aiSettings.DefaultSystemPrompt : aIClientParam.SystemPrompt;

            var chat = new Chat(ollamaApi, systemPrompt);
            chat.AllowRecursiveToolCalls = true; // Enable automatic native recursive tool execution

            // Append history
            if (aIClientParam.History != null)
            {
                foreach (var msg in aIClientParam.History)
                {
                    chat.Messages.Add(new Message
                    {
                        Role = msg.Role.ToLower() == "assistant" ? ChatRole.Assistant : ChatRole.User,
                        Content = msg.Content
                    });
                }
            }

            string nextPrompt = aIClientParam.Prompt;
            bool isFirstTurn = true;

            while (true)
            {
                IAsyncEnumerable<string> stream;
                if (isFirstTurn)
                {
                    stream = chat.SendAsync(nextPrompt,
                        tools: toolsRegistry.Tools.ToArray(),
                        cancellationToken: cancellationToken);
                    isFirstTurn = false;
                }
                else
                {
                    // Submit the tool result and prompt the model for the next turn response.
                    stream = chat.SendAsAsync(ChatRole.Tool, nextPrompt,
                        tools: toolsRegistry.Tools.ToArray(),
                        imagesAsBase64: null,
                        format: null,
                        cancellationToken: cancellationToken);
                }

                var accumulatedContent = new StringBuilder();
                await foreach (var chunk in stream)
                {
                    accumulatedContent.Append(chunk);
                    yield return new AIClientResponse
                    {
                        Content = chunk,
                        Done = false
                    };
                }

                var lastMessage = chat.Messages.LastOrDefault();
                if (lastMessage == null)
                {
                    break;
                }

                // If native tool calling was not used/supported, but the model outputs JSON tool call inside text
                if ((lastMessage.ToolCalls == null || !lastMessage.ToolCalls.Any()) &&
                    TryParseToolCallFromText(accumulatedContent.ToString(), out string toolName, out var argsMap))
                {
                    var toolCall = new Message.ToolCall
                    {
                        Function = new Message.Function
                        {
                            Name = toolName,
                            Arguments = argsMap
                        }
                    };

                    // Clean the raw JSON block from assistant's text in history
                    string fullText = accumulatedContent.ToString();
                    int jsonStart = fullText.IndexOf('{');
                    if (jsonStart >= 0)
                    {
                        lastMessage.Content = fullText.Substring(0, jsonStart).Trim();
                    }

                    // Set ToolCalls in history so the turn context makes sense to Ollama
                    lastMessage.ToolCalls = new List<Message.ToolCall> { toolCall };

                    // Execute the tool
                    var invoker = new DefaultToolInvoker();
                    string toolResultStr;
                    try
                    {
                        var toolResult = await invoker.InvokeAsync(toolCall, toolsRegistry.Tools, cancellationToken).ConfigureAwait(false);
                        toolResultStr = toolResult.Result?.ToString() ?? "";
                    }
                    catch (Exception ex)
                    {
                        toolResultStr = JsonSerializer.Serialize(new { error = ex.Message, success = false });
                    }

                    // Feed the tool result back into the chat as the next prompt
                    nextPrompt = toolResultStr;
                }
                else
                {
                    break;
                }
            }

            yield return new AIClientResponse
            {
                Done = true
            };
        }

        private static bool TryParseToolCallFromText(string text, out string toolName, out Dictionary<string, object> arguments)
        {
            toolName = null;
            arguments = null;

            if (string.IsNullOrEmpty(text)) return false;

            try
            {
                var match = Regex.Match(text, @"\{\s*""name""\s*:\s*""(?<name>[^""]+)""");
                if (match.Success)
                {
                    int start = match.Index;
                    int braceCount = 0;
                    int end = -1;
                    for (int i = start; i < text.Length; i++)
                    {
                        if (text[i] == '{') braceCount++;
                        else if (text[i] == '}')
                        {
                            braceCount--;
                            if (braceCount == 0)
                            {
                                end = i;
                                break;
                            }
                        }
                    }

                    if (end > start)
                    {
                        string json = text.Substring(start, end - start + 1);
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("name", out var nameProp))
                        {
                            toolName = nameProp.GetString();
                            arguments = new Dictionary<string, object>();
                            if (root.TryGetProperty("parameters", out var paramsProp) && paramsProp.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in paramsProp.EnumerateObject())
                                {
                                    object val = prop.Value.ValueKind switch
                                    {
                                        JsonValueKind.String => prop.Value.GetString(),
                                        JsonValueKind.Number => prop.Value.GetDouble(),
                                        JsonValueKind.True => true,
                                        JsonValueKind.False => false,
                                        JsonValueKind.Null => null,
                                        _ => prop.Value.ToString()
                                    };
                                    arguments[prop.Name] = val;
                                }
                            }
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // Ignore parsing errors and fallback to no-op
            }
            return false;
        }
    }
}