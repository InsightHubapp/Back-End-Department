namespace InsightHub.Domain.Entities
{
    public class JobOffer
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}

