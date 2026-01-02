using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace API.Rag.VectorDB.Qdrant
{
    public class QdrantVectorDB : IVectorDB
    {
        private readonly QdrantClient qdrantClient;

        public QdrantVectorDB(IConfiguration configuration)
        {
            qdrantClient = new QdrantClient("localhost", 6334);
        }

        public async Task<IEnumerable<string>> SearchAsync(string collection, float[] vector, int limit = 3)
        {
            var results = await qdrantClient.SearchAsync(collection, vector, limit: (ulong)limit);
            return results.Select(r => r.Payload["text"].ToString());
        }

        public async Task UpsertAsync(string collection, RagDocument document)
        {
            if (!await qdrantClient.CollectionExistsAsync(collection))
            {
                await qdrantClient.CreateCollectionAsync(collection, new VectorParams
                {
                    Size = 768,
                    Distance = Distance.Cosine,
                });
            }

            var point = new PointStruct
            {
                Id = (ulong)document.Id.GetHashCode(),
                Vectors = document.Vector,
                Payload = {
                    ["text"] = document.Text,
                 },
            };

            await qdrantClient.UpsertAsync(collection, new List<PointStruct> { point });
        }
    }
}