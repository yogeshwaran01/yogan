namespace API.Rag.EmbeddingGenerator
{
    public interface IEmbeddingGenerator
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}