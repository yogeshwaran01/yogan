using System.Text;
using UglyToad.PdfPig;
namespace API.Rag.TextExtractor
{

    public class PdfTextExtractor : ITextExtractor
    {
        public bool IsQualified(string contentType)
        {
            if (contentType == null) { return false; }
            return contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<string> ExtractTextAsync(Stream fileStream)
        {
            using var document = PdfDocument.Open(fileStream);
            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }

            return await Task.FromResult(sb.ToString()).ConfigureAwait(false);
        }

    }

}