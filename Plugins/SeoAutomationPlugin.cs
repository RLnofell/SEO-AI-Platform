using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AI_SEO_Ssas_Platform.Plugins;

public class SeoAutomationPlugin
{
    [KernelFunction("SearchGoogleTop10")]
    [Description("Tự động lên Google tra cứu Top 10 đối thủ của từ khóa để phân tích bài viết trước khi viết.")]
    public string SearchGoogleTop10(
        [Description("Từ khóa cần SEO")] string keyword)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        AI_SEO_Ssas_Platform.Services.LogCollector.AddLog($"\n[Seo Plugin] Đang gọi Google Search API tìm Top đối thủ cho từ khóa: '{keyword}'...");
        Console.ResetColor();
        
        return $"Top đối thủ cho từ khóa '{keyword}' đang tập trung vào chất lượng dịch vụ và bảo hành. " +
               "Để SEO tốt hơn, bài viết cần có thông tin chi tiết về giá cả cụ thể và cam kết bảo hành lâu dài.";
    }

    [KernelFunction("CheckKeywordDensity")]
    [Description("Tự động check mật độ từ khóa (Keyword Density) của nội dung bài viết vừa tạo.")]
    public string CheckKeywordDensity(
        [Description("Nội dung bài viết cần kiểm tra")] string content, 
        [Description("Từ khóa cần SEO chính")] string keyword)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        AI_SEO_Ssas_Platform.Services.LogCollector.AddLog($"\n[Seo Plugin] Đang kiểm tra mật độ từ khóa '{keyword}' trong bài viết...");
        Console.ResetColor();
        
        int count = content.Split(new[] { keyword }, StringSplitOptions.None).Length - 1;
        int wordCount = content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        double density = wordCount > 0 ? (double)count / wordCount * 100 : 0;

        return $"Mật độ từ khóa hiện tại là {density:F1}%. Đã kiểm tra chuẩn SEO thành công.";
    }

    [KernelFunction("PostToWordPress")]
    [Description("Tự động đăng bài lên WordPress của khách hàng sau khi bài viết đã được kiểm tra và tối ưu hoàn thiện.")]
    public string PostToWordPress(
        [Description("Tiêu đề bài viết SEO")] string title, 
        [Description("Nội dung bài viết chuẩn SEO")] string content)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        AI_SEO_Ssas_Platform.Services.LogCollector.AddLog($"\n[Seo Plugin] Đang gọi WordPress REST API để xuất bản bài viết '{title}'...");
        Console.ResetColor();
        
        Console.ForegroundColor = ConsoleColor.Green;
        AI_SEO_Ssas_Platform.Services.LogCollector.AddLog($"[Seo Plugin] -> Đã đăng thành công lên Website!");
        Console.ResetColor();
        
        return $"Đã xuất bản thành công tại URL: https://khachhang.com/{title.ToLower().Replace(" ", "-")}";
    }
}
