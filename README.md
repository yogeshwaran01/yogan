# Yogan (Personal AI Assistant)

- [x] Local LLM
- [x] RAG Implementation (text, Pdf)
- [x] UI Client
- [x] Tools Integration (Basic)
- [ ] Rag Implementation multiple file types
- [ ] Chunking strategies
- [ ] API based Tools Integration

## Build and Run Instructions

### Prerequisites

- .NET SDK 6.0 or higher
- Python 3.8 or higher
- Docker
- Ollama
- Qdrant

### Manual Setup (Development)

**1. Ollama**

```bash
ollama pull llama3.1:8b
```

```bash
ollama serve
```

**2. Run Qdrant**

```bash
docker run -p 6333:6333 -p 6334:6334 \
    -v $(pwd)/qdrant_storage:/qdrant/storage:z \
    qdrant/qdrant
```

**3. Run API**

```bash
dotnet run --project API
```

**4. Run Client**

```bash
ng serve
```

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
