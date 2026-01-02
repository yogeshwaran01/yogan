# Yogan (Personal AI Assistant)

## Build and Run Instructions

### Prerequisites

- .NET SDK 6.0 or higher
- Python 3.8 or higher
- Docker
- Ollama
- Qdrant

### Build the .NET API

1. Navigate to the `api/` directory.
2. Run the following command:
   ```bash
   dotnet run
   ```

### Run Qdrant

```bash
docker run -p 6333:6333 -p 6334:6334 \
    -v $(pwd)/qdrant_storage:/qdrant/storage:z \
    qdrant/qdrant
```

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
