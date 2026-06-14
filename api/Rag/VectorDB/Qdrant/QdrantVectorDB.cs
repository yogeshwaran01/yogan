using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using API.Configuration;

namespace API.Rag.VectorDB.Qdrant
{
    public class QdrantVectorDB : IVectorDB
    {
        private readonly QdrantClient qdrantClient;
        private readonly VectorDbSettings vectorDbSettings;

        public QdrantVectorDB(IOptions<VectorDbSettings> options)
        {
            vectorDbSettings = options.Value;
            var baseUrl = vectorDbSettings.Qdrant.BaseUrl;
            var port = vectorDbSettings.Qdrant.Port;
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
                var size = vectorDbSettings.Qdrant.VectorSize;
                await qdrantClient.CreateCollectionAsync(collection, new VectorParams
                {
                    Size = (ulong)size,
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