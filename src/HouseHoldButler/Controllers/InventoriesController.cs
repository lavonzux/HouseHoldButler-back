using HouseHoldButler.Extensions;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseHoldButler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoriesController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? productId)
        => this.ToActionResult(await inventoryService.GetAllAsync(productId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => this.ToActionResult(await inventoryService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryRequest request)
        => this.ToActionResult(await inventoryService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateInventoryRequest request)
        => this.ToActionResult(await inventoryService.UpdateAsync(id, request));

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PatchNote(Guid id, PatchInventoryNoteRequest request)
        => this.ToActionResult(await inventoryService.PatchNoteAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => this.ToActionResult(await inventoryService.DeleteAsync(id));
}
