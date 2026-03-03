namespace BackendApi.Entities;

public class Tag
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public ICollection<ProductTag> ProductTags { get; set; } = [];
}