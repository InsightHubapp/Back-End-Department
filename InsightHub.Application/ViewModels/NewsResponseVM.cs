namespace InsightHub.Application.ViewModels
{
    public class NewsApiResponse
    {
        public string Status { get; set; } = string.Empty;
        public int TotalResults { get; set; }
        public List<NewsResponseVM> Articles { get; set; } = new();
    }
    public class NewsResponseVM
    {
        public NewsApiSourceVM Source { get; set; } = new();
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string UrlToImage { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }
    public class NewsApiSourceVM
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

