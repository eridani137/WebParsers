using MongoDB.Bson;

namespace hobbyka;

public class ElementEntity
{
    public required string Url { get; init; }
    public string Name { get; set; } = null!;
    public int Art { get; set; }
    public decimal Price { get; set; }
}