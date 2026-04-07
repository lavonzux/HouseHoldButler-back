using HouseHoldButler.Extensions;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseHoldButler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.ToActionResult(await productService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => this.ToActionResult(await productService.GetByIdAsync(id));

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
        => this.ToActionResult(await productService.GetHistoryAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
        => this.ToActionResult(await productService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductRequest request)
        => this.ToActionResult(await productService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => this.ToActionResult(await productService.DeleteAsync(id));

    [HttpDelete("{id:guid}/force")]
    public async Task<IActionResult> ForceDelete(Guid id)
        => this.ToActionResult(await productService.ForceDeleteAsync(id));
}
