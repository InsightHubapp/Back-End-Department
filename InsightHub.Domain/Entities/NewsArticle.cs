namespace InsightHub.Domain.Entities
{
    public class NewsArticle
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string Track { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }
}

