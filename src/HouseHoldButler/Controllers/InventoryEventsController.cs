using HouseHoldButler.Extensions;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseHoldButler.Controllers;

// InventoryEvent is an append-only audit log — no update or delete endpoints.
[ApiController]
[Route("api/[controller]")]
public class InventoryEventsController(IInventoryEventService inventoryEventService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? inventoryId, [FromQuery] Guid? productId)
        => this.ToActionResult(await inventoryEventService.GetAllAsync(inventoryId, productId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => this.ToActionResult(await inventoryEventService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryEventRequest request)
        => this.ToActionResult(await inventoryEventService.CreateAsync(request));
}
