namespace InsightHub.Application.ViewModels;

public class AdzunaJob
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public AdzunaCompany company { get; set; } = new();
    public AdzunaLocation location { get; set; } = new();
    public string description { get; set; } = string.Empty;
    public string redirect_url { get; set; } = string.Empty;
    public double? salary_min { get; set; }
    public double? salary_max { get; set; }
    public string? salary_currency_code { get; set; }
    public string? created { get; set; }
}
