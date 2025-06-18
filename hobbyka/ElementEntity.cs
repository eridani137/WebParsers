namespace hobbyka;

public class ElementEntity
{
    public required string Url { get; set; }
    public string Name { get; set; } = null!;
    public int Art { get; set; }
    public string Characteristics { get; set; } = null!;
    public List<string> Colors { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<ElementVariant> Variants { get; set; } = [];
}