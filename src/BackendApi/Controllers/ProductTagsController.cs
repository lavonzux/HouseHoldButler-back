using BackendApi.Entities;
using BackendApi.Models;
using BackendApi.Requests.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductTagsController(ApplicationDbContext db, ILogger<ProductTagsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? productId, [FromQuery] Guid? tagId)
    {
        var query = db.ProductTags.AsQueryable();

        if (productId.HasValue)
            query = query.Where(pt => pt.ProductId == productId.Value);

        if (tagId.HasValue)
            query = query.Where(pt => pt.TagId == tagId.Value);

        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductTagRequest request)
    {
        var exists = await db.ProductTags.AnyAsync(
            pt => pt.ProductId == request.ProductId && pt.TagId == request.TagId);

        if (exists)
            return Conflict("This product-tag link already exists.");

        var productTag = new ProductTag
        {
            ProductId = request.ProductId,
            TagId = request.TagId
        };

        db.ProductTags.Add(productTag);
        await db.SaveChangesAsync();

        logger.LogInformation("Linked tag {TagId} to product {ProductId}", request.TagId, request.ProductId);
        return Ok(productTag);
    }

    [HttpDelete("{productId:guid}/{tagId:guid}")]
    public async Task<IActionResult> Delete(Guid productId, Guid tagId)
    {
        var productTag = await db.ProductTags.FindAsync(productId, tagId);
        if (productTag is null)
            return NotFound();

        db.ProductTags.Remove(productTag);
        await db.SaveChangesAsync();

        logger.LogInformation("Unlinked tag {TagId} from product {ProductId}", tagId, productId);
        return Ok();
    }
}
