namespace API.AIClient.Ollama.Tools
{
    public static class ToolRegisteration
    {
        public static void RegisterOllamaTools(this IToolFactory toolFactory, IServiceProvider serviceProvider)
        {
            var weatherTool = serviceProvider.GetRequiredService<WeatherTool>();
            toolFactory.RegisterTool(weatherTool);

            var timeTool = serviceProvider.GetRequiredService<TimeTool>();
            toolFactory.RegisterTool(timeTool);
        }
    }
}