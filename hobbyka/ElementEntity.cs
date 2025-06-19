using MongoDB.Bson.Serialization.Attributes;

namespace hobbyka;

[BsonIgnoreExtraElements]
public class ElementEntity
{
    public required string Url { get; set; }
    public string Name { get; set; } = null!;
    public int Art { get; set; }
    public string ShortDescription { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<string> Colors { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string Breadcrumb { get; set; } = string.Empty;
    public List<ElementVariant> Variants { get; set; } = [];
}