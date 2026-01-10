namespace API.Rag.TextExtractor
{
    public interface ITextExtractor
    {
        public bool IsQualified(string contentType);
        Task<string> ExtractTextAsync(Stream fileStream);
    }
}