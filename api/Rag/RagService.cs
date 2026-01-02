using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using API.AIClient;
using API.Rag.EmbeddingGenerator;
using API.Rag.VectorDB;

namespace API.Rag
{
    public class RagService
    {
        private readonly IEmbeddingGenerator embeddingGenerator;
        private readonly IVectorDB vectorDB;

        private readonly IAIClientFactory aIclientFactory;

        public RagService(IEmbeddingGenerator embedding, IVectorDB vector, IAIClientFactory ClientFactory)
        {
            embeddingGenerator = embedding;
            aIclientFactory = ClientFactory;
            vectorDB = vector;
        }

        public async Task AddMemoryAsync(string collection, string text)
        {
            var chunks = ChunkText(text);

            // int index = 0;
            foreach (var chunk in chunks)
            {
                var vector = await embeddingGenerator.GenerateEmbeddingAsync(chunk);

                await vectorDB.UpsertAsync(collection, new RagDocument
                {
                    Id = Guid.NewGuid(),
                    Text = chunk,
                    Vector = vector,
                });
            }
        }


        public async IAsyncEnumerable<AIClientResponse> AskAsync(string collection, string prompt, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var queryVector = await embeddingGenerator.GenerateEmbeddingAsync(prompt);

            var docs = await vectorDB.SearchAsync(collection, queryVector, 3);

            var context = string.Join("\n", docs);

            var fullPrompt = $@"
            Use the following context to answer the question.
            CONTEXT:
            {context}
            
            QUESTION: 
            {prompt}";

            IAIClient aIClient = aIclientFactory.CreateClient("llama");

            var streams = aIClient.GenerateAsync(new AIClientParam
            {
                Prompt = fullPrompt,
                Model = "llama3.1:8b"
            }, cancellationToken);

            await foreach (var chunk in streams)
            {
                yield return chunk;
            }
        }

        private List<string> ChunkText(string text, int maxTokens = 512, int overlapTokens = 128)
        {
            var chunks = new List<string>();
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");

            var current = new List<string>();
            int tokenCount = 0;

            foreach (var sentence in sentences)
            {
                int tokens = sentence.Length / 4; // token estimate

                if (tokenCount + tokens > maxTokens)
                {
                    chunks.Add(string.Join(" ", current));

                    current = current.TakeLast(3).ToList();
                    tokenCount = current.Sum(s => s.Length / 4);
                }

                current.Add(sentence);
                tokenCount += tokens;
            }

            if (current.Count > 0)
                chunks.Add(string.Join(" ", current));

            return chunks;
        }

    }
}