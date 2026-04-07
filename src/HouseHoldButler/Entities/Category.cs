namespace HouseHoldButler.Entities;

public class Category
{
    public Guid Id { get; set; }

    // Self-referencing FK for subcategories (nullable = root category)
    public Guid? ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Optional icon identifier for frontend rendering (e.g. "food", "cleaning")
    public string? Icon { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Category? Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}
