using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace AI_SEO_Ssas_Platform.Services;

#pragma warning disable SKEXP0001
#pragma warning disable CS0618
public class OllamaCustomTextEmbedding : ITextEmbeddingGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelId;

    public OllamaCustomTextEmbedding(string endpoint, string modelId)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(endpoint) };
        _modelId = modelId;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var result = new List<ReadOnlyMemory<float>>();
        
        foreach (var text in data)
        {
            var requestBody = new
            {
                model = _modelId,
                prompt = text
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(responseJson);
            
            var embeddingArray = document.RootElement.GetProperty("embedding").EnumerateArray();
            var floatList = new List<float>();
            foreach (var num in embeddingArray)
            {
                floatList.Add((float)num.GetDouble());
            }
            
            result.Add(new ReadOnlyMemory<float>(floatList.ToArray()));
        }

        return result;
    }
}
#pragma warning restore SKEXP0001
