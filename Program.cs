using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using AI_SEO_Ssas_Platform.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Đọc cấu hình
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

app.MapGet("/", () => "AI SEO Agent API is running!");

// Khởi tạo DB
LogCollector.Initialize();
await DataSeeder.SeedAsync(config);
LogCollector.AddLog("[v] Hệ thống Web API đã sẵn sàng phục vụ!");

app.MapPost("/api/agent/run", async (RunRequest request) =>
{
    LogCollector.Initialize();
    var input = request.Input;
    if (string.IsNullOrWhiteSpace(input)) return Results.BadRequest("Input không hợp lệ");

    LogCollector.AddLog($"[AI Planner] Đang phân tích yêu cầu: '{input}'...");
    LogCollector.AddLog("[AI Planner] Tự động lập kế hoạch và kích hoạt các công cụ cần thiết...");

    var kernel = KernelFactory.CreateKernel(config);
#pragma warning disable SKEXP0070
    OpenAIPromptExecutionSettings settings = new() 
    { 
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() 
    };
#pragma warning restore SKEXP0070

    string prompt = $"""
                     Nhiệm vụ của bạn là: "{input}".
                     Bạn là AI SEO Agent. Bạn đã được cung cấp sẵn các công cụ (Tools).
                     Hãy tự động suy luận và sử dụng công cụ để lấy thông tin nội bộ (giá cả, quy trình) rồi viết 1 bài đăng chuẩn SEO ngắn gọn.
                     Tuyệt đối không in ra các đoạn mã JSON.
                     """;

    try 
    {
        var result = await kernel.InvokePromptAsync(prompt, new(settings));
        string responseText = result.ToString();
        
        string finalArticle = "";
        string densityResultText = "";
        string postResultText = "";

        if (responseText.Contains("\"name\"") && responseText.Contains("SeoAutomationPlugin"))
        {
            LogCollector.AddLog("\n[AI Planner Fallback] AI đã ra quyết định gọi Tool nhưng xuất dưới dạng JSON.");
            LogCollector.AddLog("[AI Planner Fallback] Đang tự động trích xuất lệnh và thực thi hàm C#...\n");
            
            string keyword = input;
            if (input.Contains("Thắng Hiền", StringComparison.OrdinalIgnoreCase) || responseText.Contains("Thắng Hiền")) 
                keyword = "Thắng Hiền";
            else if (input.Contains("Đạt Phát", StringComparison.OrdinalIgnoreCase) || responseText.Contains("Đạt Phát")) 
                keyword = "Đạt Phát";
            
            LogCollector.AddLog($"\n[Agentic Pipeline] Bước 1: Gọi hàm SearchInternalKnowledge(query: '{keyword}')");
            var ragPlugin = kernel.Plugins["RagPlugin"];
            var ragResult = await kernel.InvokeAsync(ragPlugin["SearchInternalKnowledge"], new() { ["query"] = keyword });
            
            LogCollector.AddLog($"[Agentic Pipeline] Bước 2: Gọi hàm SearchGoogleTop10(keyword: '{keyword}')");
            var seoPlugin = kernel.Plugins["SeoAutomationPlugin"];
            var googleResult = await kernel.InvokeAsync(seoPlugin["SearchGoogleTop10"], new() { ["keyword"] = keyword });
            
            LogCollector.AddLog("\n[Agentic Pipeline] Bước 3: Yêu cầu AI viết bài dựa trên dữ liệu RAG và Google...");
            string finalPrompt = $@"Bạn là một Chuyên gia SEO AI.
Nhiệm vụ: Viết 1 bài đăng chuẩn SEO thật lôi cuốn, có gạch đầu dòng rõ ràng cho yêu cầu: '{input}'.

DỮ LIỆU BẮT BUỘC PHẢI DÙNG:
1. Dữ liệu công ty (Từ Database): {ragResult}
2. Dữ liệu thị trường (Từ Google): {googleResult}

Yêu cầu: 
- Viết bài trực tiếp, không in JSON.
- Đưa thông tin giá cả/bảo hành từ Database vào bài một cách tự nhiên.
- Độ dài dưới 250 từ.";
            var articleResult = await kernel.InvokePromptAsync(finalPrompt);
            finalArticle = articleResult.ToString();
            
            LogCollector.AddLog("\n[Agentic Pipeline] Bước 4: Gọi hàm CheckKeywordDensity...");
            var densityResult = await kernel.InvokeAsync(seoPlugin["CheckKeywordDensity"], new() { ["content"] = finalArticle, ["keyword"] = keyword });
            densityResultText = densityResult.ToString() ?? "";

            LogCollector.AddLog("\n[Agentic Pipeline] Bước 5: Gọi hàm PostToWordPress...");
            string title = $"Dịch vụ {keyword} Tốt Nhất";
            var postResult = await kernel.InvokeAsync(seoPlugin["PostToWordPress"], new() { ["title"] = title, ["content"] = finalArticle });
            postResultText = postResult.ToString() ?? "";
            
            LogCollector.AddLog("\nBÁO CÁO TỔNG KẾT TỪ AI AGENT:");
            LogCollector.AddLog(densityResultText);
            LogCollector.AddLog(postResultText);
        }
        else
        {
            LogCollector.AddLog("\nBÁO CÁO CÔNG VIỆC TỪ AI AGENT:");
            LogCollector.AddLog(responseText);
            finalArticle = responseText;
        }

        return Results.Ok(new RunResponse(LogCollector.GetLogs(), finalArticle, densityResultText, postResultText));
    }
    catch (Exception ex)
    {
        LogCollector.AddLog($"[LỖI AI]: Có thể API Key chưa chính xác. Chi tiết: {ex.Message}");
        return Results.BadRequest(new { Error = ex.Message, Logs = LogCollector.GetLogs() });
    }
});

app.Run();

public record RunRequest(string Input);
public record RunResponse(List<string> Logs, string FinalArticle, string DensityResult, string PostResult);
