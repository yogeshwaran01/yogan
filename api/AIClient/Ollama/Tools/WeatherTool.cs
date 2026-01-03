using System.Text.Json;
using OllamaSharp.Models.Chat;
using static OllamaSharp.Models.Chat.Message;

namespace API.AIClient.Ollama.Tools
{
    public class WeatherTool : IOllamaTool
    {

        public WeatherTool()
        {
        }

        public string Name => "get_current_weather";

        public string Description => "Get the current weather of the given location";

        public Dictionary<string, Property> Parameters
        {
            get
            {
                return new Dictionary<string, Property>
                {
                    ["location"] = new Property
                    {
                        Type = "string",
                        Description = "The city and state eg: Chennai, TN"
                    }
                };
            }
        }

        public string[] Required => ["location"];

        public string Action(ToolCall toolCall)
        {
            string location = GetArgument(toolCall.Function.Arguments, "location", "Unknown");
            var rnd = new Random();
            return $"{rnd.Next(20, 35)} , Rain in {location}";
        }

        private string GetArgument(IDictionary<string, object> args, string key, string defaultValue)
        {
            if (args == null || !args.ContainsKey(key))
            {
                return defaultValue;
            }

            var value = args[key];

            if (value == null) return defaultValue;

            // Handle JsonElement (common if using System.Text.Json)
            if (value is JsonElement element)
            {
                return element.ToString();
            }

            return value.ToString() ?? defaultValue;
        }
    }
}