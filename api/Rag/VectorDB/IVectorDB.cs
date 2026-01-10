namespace API.Rag.VectorDB
{
    public interface IVectorDB
    {
        Task UpsertAsync(string collection, RagDocument document);
        Task<IEnumerable<string>> SearchAsync(string collection, float[] vector, int limit = 3);
        Task<IEnumerable<string>> ListCollectionsAsync();
    }
}