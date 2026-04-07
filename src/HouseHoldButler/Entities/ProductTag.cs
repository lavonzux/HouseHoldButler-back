namespace HouseHoldButler.Entities;

// Explicit join entity for the many-to-many between Product and Tag.
// Using an explicit class (rather than EF's implicit join) keeps the door open
// for adding extra columns later (e.g. tagged_by, tagged_at).
public class ProductTag
{
    public Guid ProductId { get; set; }
    public Guid TagId { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}