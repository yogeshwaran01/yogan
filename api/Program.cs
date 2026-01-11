using API.AIClient;
using API.AIClient.Gemini;
using API.AIClient.Ollama;
using API.AIClient.Ollama.Tools;
using API.Rag;
using API.Rag.EmbeddingGenerator;
using API.Rag.EmbeddingGenerator.Ollama;
using API.Rag.TextExtractor;
using API.Rag.VectorDB;
using API.Rag.VectorDB.Qdrant;
using Google.GenAI;
using OllamaSharp;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<OllamaClient>();
builder.Services.AddScoped<GoogleClient>();
builder.Services.AddScoped<IAIClientFactory, AIClientFactory>();
builder.Services.AddScoped<IEmbeddingGenerator, OllamaEmbeddingGenerator>();
builder.Services.AddScoped<IVectorDB, QdrantVectorDB>();
builder.Services.AddScoped<RagService>();
builder.Services.AddControllers();
builder.Services.AddSingleton<WeatherTool>();
builder.Services.AddSingleton<TimeTool>();
builder.Services.AddSingleton<IToolFactory, ToolFactory>();
builder.Services.AddSingleton<ITextExtractor, PdfTextExtractor>();
builder.Services.AddSingleton<IOllamaApiClient>(sp =>
{
    var baseurl = builder.Configuration["AI:Ollama:BaseUrl"];
    var client = new OllamaApiClient(baseurl);
    return client;
});

builder.Services.AddSingleton<Client>(sp =>
{
    var apiKey = builder.Configuration["AI:Google:ApiKey"];
    var client = new Client(apiKey: apiKey);
    return client;
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Yogan API",
        Version = "1",
    });
});

var app = builder.Build();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true) // allow any origin
    .AllowCredentials()); // allow credentials

app.Services.GetRequiredService<IToolFactory>().RegisterOllamaTools(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ollama AI API V1");
        c.RoutePrefix = string.Empty;
    });
    app.MapOpenApi();
}

app.MapControllers();

app.UseHttpsRedirection();
app.Run();