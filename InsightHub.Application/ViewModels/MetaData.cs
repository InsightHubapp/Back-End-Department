namespace InsightHub.Application.ViewModels;

public class MetaData
{
    public int Total { get; set; }
    public int PageTotal { get; set; }
    public int Page { get; set; }
    public int LastPage { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public Links Links { get; set; } = new();
}
