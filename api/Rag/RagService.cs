using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using API.AIClient;
using API.Rag.EmbeddingGenerator;
using API.Rag.TextExtractor;
using API.Rag.VectorDB;

namespace API.Rag
{
    public class RagService
    {
        private readonly IEmbeddingGenerator embeddingGenerator;
        private readonly IVectorDB vectorDB;

        private readonly IAIClientFactory aIclientFactory;

        private readonly IEnumerable<ITextExtractor> textExtractors;

        public RagService(IEmbeddingGenerator embedding, IVectorDB vector, IAIClientFactory ClientFactory, IEnumerable<ITextExtractor> textExtractors)
        {
            embeddingGenerator = embedding;
            aIclientFactory = ClientFactory;
            this.textExtractors = textExtractors;
            vectorDB = vector;
        }

        public async Task AddMemoryAsync(AIClientParam aiClientParam)
        {
            if (aiClientParam == null) { return; }
            var chunks = ChunkText(aiClientParam.Context);
            foreach (var chunk in chunks)
            {
                var vector = await embeddingGenerator.GenerateEmbeddingAsync(chunk).ConfigureAwait(false);

                await vectorDB.UpsertAsync(aiClientParam.StoreName, new RagDocument
                {
                    Id = Guid.NewGuid(),
                    Text = chunk,
                    Vector = vector,
                }).ConfigureAwait(false);
            }
        }

        public async Task AddMemoryFromFileAsync(AIClientParam aiClientParam)
        {
            if (aiClientParam == null) { return; }
            var file = aiClientParam.FormFile;
            using var stream = file.OpenReadStream();
            var extractor = textExtractors.FirstOrDefault(e => e.IsQualified(file.ContentType)) ?? throw new NotSupportedException($"Unsupported content type: {file.ContentType}");
            var text = await extractor.ExtractTextAsync(stream).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text)) { return; }
            var normalizePdfText = NormalizePdfText(text);
            aiClientParam.Context = normalizePdfText;
            await AddMemoryAsync(aiClientParam).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetStoresAsync()
        {
            return await vectorDB.ListCollectionsAsync().ConfigureAwait(false);
        }


        public async IAsyncEnumerable<AIClientResponse> AskAsync(AIClientParam param, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (param == null) { throw new ArgumentNullException(nameof(param)); }
            var queryVector = await embeddingGenerator.GenerateEmbeddingAsync(param.Prompt).ConfigureAwait(false);
            var context = string.Empty;
            if (param.IsRagEnabled)
            {
                var docs = await vectorDB.SearchAsync(param.StoreName, queryVector, 3).ConfigureAwait(false);
                context = string.Join("\n", docs);
            }
            string fullPrompt;
            if (!string.IsNullOrEmpty(context))
            {
                fullPrompt = $@"
            Use the following context to answer the question.
            CONTEXT:
            {context}
            
            QUESTION: 
            {param.Prompt}";

            }
            else
            {
                fullPrompt = param.Prompt;
            }
            IAIClient aIClient = aIclientFactory.CreateClient(param.Client);
            var streams = aIClient.GenerateAsync(new AIClientParam
            {
                Prompt = fullPrompt,
                Model = param.Model,
            }, cancellationToken);

            await foreach (var chunk in streams)
            {
                yield return chunk;
            }
        }

        private static List<string> ChunkText(string text, int maxTokens = 512)
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

        private static string NormalizePdfText(string text)
        {
            return Regex
                .Replace(text, @"\n{2,}", "\n")
                .Replace("\r", "")
                .Trim();
        }
    }
}