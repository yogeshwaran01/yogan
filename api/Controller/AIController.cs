using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using API.AIClient;
using API.Rag;
using API.History;
using API.Configuration;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly RagService ragService;
        private readonly IHistoryService historyService;
        private readonly AiSettings aiSettings;

        public AIController(RagService rag, IHistoryService history, IOptions<AiSettings> options)
        {
            ragService = rag;
            historyService = history;
            aiSettings = options.Value;
        }

        [HttpPost("generate")]
        public async Task Generate([FromBody] AIClientParam aiClientParam, CancellationToken cancellationToken)
        {
            if (aiClientParam == null)
            {
                Response.StatusCode = 400;
                return;
            }

            ApplyDefaults(aiClientParam);
            var isValid = aiClientParam.IsValidForGenerate();
            if (!isValid)
            {
                Response.StatusCode = 400;
                return;
            }

            // Load history if ConversationId is provided
            if (!string.IsNullOrEmpty(aiClientParam.ConversationId))
            {
                var history = await historyService.GetHistoryAsync(aiClientParam.ConversationId).ConfigureAwait(false);
                aiClientParam.History = history;
            }

            Response.ContentType = "application/json";

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            var accumulatedResponse = new StringBuilder();

            await foreach (var item in ragService.AskAsync(aiClientParam, cancellationToken))
            {
                if (!string.IsNullOrEmpty(item.Content))
                {
                    accumulatedResponse.Append(item.Content);
                }
                var json = System.Text.Json.JsonSerializer.Serialize(item, options);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }

            // Save conversation history
            if (!string.IsNullOrEmpty(aiClientParam.ConversationId))
            {
                await historyService.AddMessageAsync(aiClientParam.ConversationId, new ChatMessage
                {
                    Role = "user",
                    Content = aiClientParam.Prompt
                }).ConfigureAwait(false);

                await historyService.AddMessageAsync(aiClientParam.ConversationId, new ChatMessage
                {
                    Role = "assistant",
                    Content = accumulatedResponse.ToString()
                }).ConfigureAwait(false);
            }
        }

        [HttpPost("context/text")]
        public async Task<IActionResult> AddContext([FromBody] AIClientParam aiClientParam)
        {
            if (aiClientParam == null) { return BadRequest("Invalid parameters for adding context"); }
            ApplyDefaults(aiClientParam);
            var isValid = aiClientParam.IsValidForContext();
            if (!isValid) { return BadRequest("Invalid parameters for adding context."); }
            await ragService.AddMemoryAsync(aiClientParam).ConfigureAwait(false);
            return Ok();
        }

        [HttpPost("context/file")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> AddFileContext([FromForm] AIClientParam aIClientParam)
        {
            if (aIClientParam == null) { return BadRequest("Invalid parameters for adding file context"); }
            ApplyDefaults(aIClientParam);
            var isValid = aIClientParam.IsValidForFileContext();
            if (!isValid) { return BadRequest("Invalid parameters for adding file context."); }
            await ragService.AddMemoryFromFileAsync(aIClientParam).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetStores()
        {
            var stores = await ragService.GetStoresAsync().ConfigureAwait(false);
            return Ok(stores);
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            return Ok(new
            {
                DefaultClient = aiSettings.DefaultClient,
                DefaultModel = aiSettings.DefaultModel,
                DefaultStoreName = aiSettings.DefaultStoreName,
                DefaultSystemPrompt = aiSettings.DefaultSystemPrompt,
                Clients = aiSettings.AvailableClients,
                Models = aiSettings.Models
            });
        }

        [HttpGet("history/{conversationId}")]
        public async Task<IActionResult> GetHistory(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return BadRequest("Conversation ID is required.");
            }
            var history = await historyService.GetHistoryAsync(conversationId).ConfigureAwait(false);
            return Ok(history);
        }

        [HttpDelete("history/{conversationId}")]
        public async Task<IActionResult> ClearHistory(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return BadRequest("Conversation ID is required.");
            }
            await historyService.ClearHistoryAsync(conversationId).ConfigureAwait(false);
            return Ok();
        }

        private void ApplyDefaults(AIClientParam param)
        {
            if (string.IsNullOrEmpty(param.Client)) param.Client = aiSettings.DefaultClient;
            if (string.IsNullOrEmpty(param.Model)) param.Model = aiSettings.DefaultModel;
            if (string.IsNullOrEmpty(param.StoreName)) param.StoreName = aiSettings.DefaultStoreName;
            if (string.IsNullOrEmpty(param.SystemPrompt)) param.SystemPrompt = aiSettings.DefaultSystemPrompt;
        }
    }
}