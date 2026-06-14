namespace API.Configuration
{
    public class VectorDbSettings
    {
        public QdrantSettings Qdrant { get; set; } = new();
    }

    public class QdrantSettings
    {
        public string BaseUrl { get; set; } = "localhost";
        public int Port { get; set; } = 6334;
        public int VectorSize { get; set; } = 768;
    }
}
