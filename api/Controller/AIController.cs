using API.AIClient;
using API.Rag;
using Microsoft.AspNetCore.Mvc;

namespace API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly RagService ragService;

        public AIController(RagService rag)
        {
            ragService = rag;
        }

        [HttpPost("generate")]
        public async Task Generate([FromBody] AIClientParam aiClientParam, CancellationToken cancellationToken)
        {
            if (aiClientParam == null) 
            { 
                Response.StatusCode = 400;
                return;
            }
            
            aiClientParam.AddDefaults();
            var isValid = aiClientParam.IsValidForGenerate();
            if (!isValid) 
            { 
                Response.StatusCode = 400; 
                return;
            }

            Response.ContentType = "application/json"; // Or application/x-ndjson if strictly needed, but json is fine for browser fetch
            
            // Use camelCase to match client expectations
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };

            await foreach (var item in ragService.AskAsync(aiClientParam, cancellationToken))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(item, options);
                await Response.WriteAsync(json + "\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpPost("context/text")]
        public async Task<IActionResult> AddContext([FromBody] AIClientParam aiClientParam)
        {
            if (aiClientParam == null) { return BadRequest("Invalid parameters for adding context"); }
            aiClientParam.AddDefaults();
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
            aIClientParam.AddDefaults();
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
    }
}