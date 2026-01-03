using OllamaSharp.Models.Chat;

namespace API.AIClient.Ollama.Tools
{
    public interface IToolFactory
    {
        public void RegisterTool(IOllamaTool tool);

        public List<Tool> GetTools();

        public IOllamaTool GetTool(string tollName);
    }


    public class ToolFactory : IToolFactory
    {
        private readonly List<IOllamaTool> ollamaTools = [];

        public IOllamaTool GetTool(string tollName)
        {
            return ollamaTools.FirstOrDefault(
    t => string.Equals(t.Name, tollName, StringComparison.OrdinalIgnoreCase)
);

        }

        public List<Tool> GetTools()
        {
            return ollamaTools.Select(t => ToTool(t)).ToList();
        }

        public void RegisterTool(IOllamaTool tool)
        {
            if (ollamaTools.Any(t => t.Name == tool.Name)) { return; }
            ollamaTools.Add(tool);
        }

        private Tool ToTool(IOllamaTool ollamaTool)
        {

            return new Tool
            {
                Type = "function",
                Function = new Function
                {
                    Name = ollamaTool.Name,
                    Description = ollamaTool.Description,
                    Parameters = new Parameters
                    {
                        Type = "object",
                        Properties = ollamaTool.Parameters,
                        Required = ollamaTool.Required
                    }

                }
            };
        }
    }
}