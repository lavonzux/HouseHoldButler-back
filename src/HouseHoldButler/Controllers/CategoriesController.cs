using HouseHoldButler.Extensions;
using HouseHoldButler.Requests.Inventory;
using HouseHoldButler.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HouseHoldButler.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => this.ToActionResult(await categoryService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => this.ToActionResult(await categoryService.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
        => this.ToActionResult(await categoryService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request)
        => this.ToActionResult(await categoryService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => this.ToActionResult(await categoryService.DeleteAsync(id));

    // 取得所有分類 (供前端下拉選單使用)
    [HttpGet("getCategories")]
    public async Task<IActionResult> GetCategories()
        => this.ToActionResult(await categoryService.GetCategoryDropdownAsync());
}
