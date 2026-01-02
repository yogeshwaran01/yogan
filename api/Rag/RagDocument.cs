namespace API.Rag
{
    public class RagDocument
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public float[] Vector { get; set; }

        public Dictionary<string, object> Metadata { get; set; }
    }
}