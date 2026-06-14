using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using OllamaSharp;

namespace API.AIClient.Ollama.Tools
{
    /// <summary>
    /// Registers and manages tool implementations for Ollama AI client integration.
    /// </summary>
    public static class ToolRegistration
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static ToolRegistration()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; YoganAgent/1.0)");
        }

        /// <summary>
        /// Helps to get the current date and time.
        /// </summary>
        [OllamaTool]
        public static string GetDateTime()
        {
            Dictionary<string, string> date = new Dictionary<string, string>();
            date.Add("datetime", DateTime.Now.ToString("O"));
            return JsonSerializer.Serialize(date);
        }

        /// <summary>
        /// Returns basic server information.
        /// </summary>
        [OllamaTool]
        public static string GetServerInfo()
        {
            var info = new
            {
                Environment.MachineName,
                OS = Environment.OSVersion.ToString(),
                Environment.ProcessorCount,
                TimeUtc = DateTime.UtcNow.ToString("O")
            };
            return JsonSerializer.Serialize(info);
        }

        /// <summary>
        /// Evaluates a mathematical expression (e.g. "2 * (3 + 4)") and returns the numeric result.
        /// </summary>
        /// <param name="expression">The mathematical expression to evaluate, e.g. "3.5 * 12 - 4"</param>
        [OllamaTool]
        public static string EvaluateMath(string expression)
        {
            try
            {
                using (var dt = new System.Data.DataTable())
                {
                    var result = dt.Compute(expression, "");
                    return JsonSerializer.Serialize(new { expression, result = result.ToString(), success = true });
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { expression, error = ex.Message, success = false });
            }
        }

        /// <summary>
        /// Gets the current weather forecast for a specific city location.
        /// </summary>
        /// <param name="city">The name of the city to look up, e.g. London, Tokyo, Chennai</param>
        [OllamaTool]
        public static string GetWeather(string city)
        {
            try
            {
                // 1. Geocode city name to coordinates
                var geocodeUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(city)}&count=1";
                var geocodeResponse = httpClient.GetStringAsync(geocodeUrl).GetAwaiter().GetResult();
                using var geocodeDoc = JsonDocument.Parse(geocodeResponse);
                
                if (!geocodeDoc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    return JsonSerializer.Serialize(new { city, error = "City location not found.", success = false });
                }

                var location = results[0];
                var lat = location.GetProperty("latitude").GetDouble();
                var lon = location.GetProperty("longitude").GetDouble();
                var cityName = location.GetProperty("name").GetString();
                var country = location.GetProperty("country").GetString();

                // 2. Fetch weather details
                var weatherUrl = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation,wind_speed_10m";
                var weatherResponse = httpClient.GetStringAsync(weatherUrl).GetAwaiter().GetResult();
                using var weatherDoc = JsonDocument.Parse(weatherResponse);
                
                var current = weatherDoc.RootElement.GetProperty("current");
                var temperature = current.GetProperty("temperature_2m").GetDouble();
                var humidity = current.GetProperty("relative_humidity_2m").GetDouble();
                var apparentTemp = current.GetProperty("apparent_temperature").GetDouble();
                var windSpeed = current.GetProperty("wind_speed_10m").GetDouble();

                var result = new
                {
                    City = cityName,
                    Country = country,
                    Latitude = lat,
                    Longitude = lon,
                    TemperatureCelsius = temperature,
                    ApparentTemperatureCelsius = apparentTemp,
                    HumidityPercent = humidity,
                    WindSpeedKmh = windSpeed,
                    Success = true
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { city, error = ex.Message, success = false });
            }
        }

        /// <summary>
        /// Retrieves the currency exchange rates or converts money from one currency to another using real-time market data.
        /// </summary>
        /// <param name="baseCurrency">The base currency code (3 letters), e.g. USD, EUR, INR</param>
        /// <param name="targetCurrency">The target currency code (3 letters), e.g. EUR, JPY, CAD</param>
        /// <param name="amount">The amount of money to convert.</param>
        [OllamaTool]
        public static string ConvertCurrency(string baseCurrency, string targetCurrency, double amount = 1.0)
        {
            try
            {
                var baseCode = baseCurrency.ToUpper().Trim();
                var targetCode = targetCurrency.ToUpper().Trim();
                
                var url = $"https://open.er-api.com/v6/latest/{baseCode}";
                var response = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(response);
                
                if (doc.RootElement.GetProperty("result").GetString() != "success")
                {
                    return JsonSerializer.Serialize(new { baseCurrency, targetCurrency, error = "Failed to retrieve exchange rates.", success = false });
                }

                var rates = doc.RootElement.GetProperty("rates");
                if (!rates.TryGetProperty(targetCode, out var rateElement))
                {
                    return JsonSerializer.Serialize(new { baseCurrency, targetCurrency, error = $"Target currency '{targetCode}' is not supported.", success = false });
                }

                var rate = rateElement.GetDouble();
                var convertedAmount = amount * rate;

                var result = new
                {
                    BaseCurrency = baseCode,
                    TargetCurrency = targetCode,
                    OriginalAmount = amount,
                    ExchangeRate = rate,
                    ConvertedAmount = convertedAmount,
                    LastUpdated = doc.RootElement.GetProperty("time_last_update_utc").GetString(),
                    Success = true
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { baseCurrency, targetCurrency, error = ex.Message, success = false });
            }
        }

        /// <summary>
        /// Searches Wikipedia articles for a given topic and returns summaries of matching pages.
        /// </summary>
        /// <param name="query">The search term or topic to search on Wikipedia, e.g. "Quantum Computing", "Albert Einstein"</param>
        [OllamaTool]
        public static string SearchWikipedia(string query)
        {
            try
            {
                var url = $"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&format=json&utf8=1";
                var response = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(response);
                
                if (!doc.RootElement.TryGetProperty("query", out var queryProp) || 
                    !queryProp.TryGetProperty("search", out var searchList))
                {
                    return JsonSerializer.Serialize(new { query, error = "No search results found.", success = false });
                }

                var results = new List<object>();
                int count = Math.Min(searchList.GetArrayLength(), 3); // Return top 3 articles
                for (int i = 0; i < count; i++)
                {
                    var item = searchList[i];
                    results.Add(new
                    {
                        Title = item.GetProperty("title").GetString(),
                        Snippet = item.GetProperty("snippet").GetString().Replace("<span class=\"searchmatch\">", "").Replace("</span>", ""),
                        PageId = item.GetProperty("pageid").GetInt64(),
                        Link = $"https://en.wikipedia.org/?curid={item.GetProperty("pageid").GetInt64()}"
                    });
                }

                return JsonSerializer.Serialize(new { query, results, success = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { query, error = ex.Message, success = false });
            }
        }

        /// <summary>
        /// Returns system metrics from the host server, including operating system version, processor count, memory usage, and disk space.
        /// </summary>
        [OllamaTool]
        public static string GetSystemMetrics()
        {
            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                var ramMemoryUsed = process.PrivateMemorySize64 / (1024.0 * 1024.0); // in MB
                
                var drives = System.IO.DriveInfo.GetDrives();
                var driveMetrics = new List<object>();
                foreach (var d in drives)
                {
                    if (d.IsReady)
                    {
                        driveMetrics.Add(new
                        {
                            Name = d.Name,
                            TotalSizeGB = d.TotalSize / (1024.0 * 1024.0 * 1024.0),
                            FreeSpaceGB = d.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0),
                            Format = d.DriveFormat
                        });
                    }
                }

                var result = new
                {
                    OS = Environment.OSVersion.ToString(),
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount,
                    AppMemoryUsageMB = ramMemoryUsed,
                    Drives = driveMetrics,
                    Success = true
                };

                return JsonSerializer.Serialize(result);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { error = ex.Message, success = false });
            }
        }
    }
}