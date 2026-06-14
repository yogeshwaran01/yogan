using System.Collections.Generic;

namespace API.AIClient.Ollama.Tools
{
    public class OllamaToolsRegistry
    {
        public IEnumerable<object> Tools { get; }

        public OllamaToolsRegistry()
        {
            Tools = new object[]
            {
                new GetDateTimeTool(),
                new GetServerInfoTool(),
                new EvaluateMathTool(),
                new GetWeatherTool(),
                new ConvertCurrencyTool(),
                new SearchWikipediaTool(),
                new GetSystemMetricsTool()
            };
        }
    }
}
