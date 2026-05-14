using AI_SEO_Ssas_Platform.Services;
using AI_SEO_Ssas_Platform.Plugins;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite default port
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 2. Đăng ký SignalR
builder.Services.AddSignalR();

// 3. Đăng ký Services & Plugins
builder.Services.AddSingleton<ILogCollector, LogCollector>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// 4. Đăng ký Semantic Kernel qua DI
builder.Services.AddScoped(sp => 
{
    var config = sp.GetRequiredService<IConfiguration>();
    var kernel = KernelFactory.CreateKernel(config);
    
    // Thêm các Plugin vào Kernel
    kernel.Plugins.AddFromType<RagPlugin>("RagPlugin");
    kernel.Plugins.AddFromType<SeoAutomationPlugin>("SeoAutomationPlugin");
    
    return kernel;
});

var app = builder.Build();

app.UseCors("AllowAll");

// Khởi tạo Database khi Start
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    await DataSeeder.SeedAsync(config);
}

// Map Hubs
app.MapHub<AgentHub>("/agentHub");

// Endpoints
app.MapGet("/", () => "AI SEO Agent API is running with SignalR support!");

app.MapPost("/api/agent/run", async (RunRequest request, IAgentOrchestrator orchestrator) =>
{
    try 
    {
        var result = await orchestrator.RunAgentAsync(request.Input);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.Run();

public record RunRequest(string Input);
