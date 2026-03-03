using BackendApi.Entities;
using BackendApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ApplicationDbContext db, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await db.Products.OrderBy(p => p.Name).ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CategoryId = request.CategoryId,
            Barcode = request.Barcode,
            AvgConsumptionRate = request.AvgConsumptionRate,
            LowStockThreshold = request.LowStockThreshold,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        logger.LogInformation("Created product {Id} ({Name})", product.Id, product.Name);
        return Ok(product);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Barcode = request.Barcode;
        product.AvgConsumptionRate = request.AvgConsumptionRate;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync();

        logger.LogInformation("Updated product {Id}", id);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await db.Products
            .Include(p => p.Inventories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound();

        if (product.Inventories.Any())
            return Conflict("Cannot delete a product that has inventory records.");

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        logger.LogInformation("Deleted product {Id}", id);
        return Ok();
    }
}
