using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();

builder.AddOpenAIChatCompletion(
    modelId: "llama3.2", 
    apiKey: "no-key", 
    endpoint: new Uri("http://localhost:11434/v1")
);

var kernel = builder.Build();

string promptString = """
                      Bạn là một chuyên gia SEO hàng đầu tại Việt Nam. 
                      Hãy viết 3 tiêu đề bài viết (Title) và 3 đoạn mô tả (Meta Description) 
                      tối ưu cho từ khóa: {{$keyword}}
                      Yêu cầu: Nội dung hấp dẫn, chứa từ khóa, phù hợp với khách hàng tại {{$location}}.
                      """;

var seoFunction = kernel.CreateFunctionFromPrompt(promptString);

var arguments = new KernelArguments
{
    ["keyword"] = "Dịch vụ cho thuê xe cẩu 10 tấn",
    ["location"] = "Long An"
};

var result = await kernel.InvokeAsync(seoFunction, arguments);

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("========== KẾT QUẢ SEO TỰ ĐỘNG ==========");
Console.WriteLine(result.ToString());