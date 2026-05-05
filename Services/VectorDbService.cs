using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.Sqlite;

namespace AI_SEO_Ssas_Platform.Services;

public static class VectorDbService
{
    public static ISemanticTextMemory Memory { get; private set; } = null!;

    public static void Initialize(IConfiguration config)
    {
        #pragma warning disable SKEXP0001
        #pragma warning disable CS0618
        
        string ollamaUrl = config["AI:Endpoint"]?.Replace("/v1", "") ?? "http://localhost:11434";
        string embedModel = config["AI:EmbeddingModelId"] ?? "nomic-embed-text";
        string dbConnection = config["Database:VectorDbConnectionString"] ?? "vector_database.db";
        
        var customOllamaEmbedding = new OllamaCustomTextEmbedding(ollamaUrl, embedModel);
        
        var store = SqliteMemoryStore.ConnectAsync(dbConnection).GetAwaiter().GetResult();
        
        Memory = new SemanticTextMemory(store, customOllamaEmbedding);
        #pragma warning restore CS0618
        #pragma warning restore SKEXP0001
    }
}
