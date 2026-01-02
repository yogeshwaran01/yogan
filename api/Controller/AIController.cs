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
        public IAsyncEnumerable<AIClientResponse> Generate([FromBody] AIClientParam aIClientParam, CancellationToken cancellationToken)
        {
            string collectionName = aIClientParam.StoreName ?? "store";
            return ragService.AskAsync(collectionName, aIClientParam.Prompt, cancellationToken);
        }

        [HttpPost("context")]
        public Task AddContext([FromBody] AIClientParam context, CancellationToken cancellation)
        {
            string collectionName = context.StoreName ?? "store";
            return ragService.AddMemoryAsync(collectionName, context.Context);
        }

    }
}