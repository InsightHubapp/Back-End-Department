using System.Text.Json.Serialization;

public class ExploreRequestDto
{
    [JsonPropertyName("filters")]
    public ExploreFilters Filters { get; set; } = new();

}

public class ExploreFilters
{
    [JsonPropertyName("title")]
    public TitleFilter? Title { get; set; }

    [JsonPropertyName("field_label")]
    public List<string>? FieldLabel { get; set; }

    [JsonPropertyName("salary_avg")]
    public SalaryFilter? SalaryAvg { get; set; }
}

public class TitleFilter
{
    [JsonPropertyName("contains")]
    public string? Contains { get; set; }
}

public class SalaryFilter
{
    [JsonPropertyName(">=")]
    public float? Min { get; set; }

    [JsonPropertyName("<=")]
    public float? Max { get; set; }
}