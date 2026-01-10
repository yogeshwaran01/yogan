using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace API.Rag.VectorDB.Qdrant
{
    public class QdrantVectorDB : IVectorDB
    {
        private readonly QdrantClient qdrantClient;

        public QdrantVectorDB(IConfiguration configuration)
        {
            var qdrantConfig = configuration.GetSection("VectorDB:Qdrant");
            var baseUrl = qdrantConfig["BaseUrl"];
            var port = int.Parse(qdrantConfig["Port"]);
            qdrantClient = new QdrantClient(baseUrl, port);
        }

        public async Task<IEnumerable<string>> SearchAsync(string collection, float[] vector, int limit = 3)
        {
            var results = await qdrantClient.SearchAsync(collection, vector, limit: (ulong)limit).ConfigureAwait(false);
            return results.Select(r => r.Payload["text"].ToString());
        }

        public async Task UpsertAsync(string collection, RagDocument document)
        {
            if (!await qdrantClient.CollectionExistsAsync(collection).ConfigureAwait(false))
            {
                await qdrantClient.CreateCollectionAsync(collection, new VectorParams
                {
                    Size = 768,
                    Distance = Distance.Cosine,
                }).ConfigureAwait(false);
            }

            var point = new PointStruct
            {
                Id = (ulong)document.Id.GetHashCode(),
                Vectors = document.Vector,
                Payload = {
                    ["text"] = document.Text,
                 },
            };

            await qdrantClient.UpsertAsync(collection, new List<PointStruct> { point }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> ListCollectionsAsync()
        {
            return await qdrantClient.ListCollectionsAsync().ConfigureAwait(false);
        }
    }
}