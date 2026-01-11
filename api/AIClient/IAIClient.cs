namespace API.AIClient
{
    public interface IAIClient
    {
        IAsyncEnumerable<AIClientResponse> GenerateAsync(AIClientParam aIClientParam, CancellationToken cancellationToken);

    }
}