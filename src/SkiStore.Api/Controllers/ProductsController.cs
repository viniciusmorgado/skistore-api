using Microsoft.AspNetCore.Mvc;
using SkiStore.Core.Base.Interfaces;
using SkiStore.Core.Entities;
using SkiStore.Core.Specs;

namespace SkiStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IGenericRepository<Product> repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> Get([FromQuery] ProductSpecParams specParams)
    {
        var spec = new ProductSpec(specParams);
        var products = await repository.GetAllWithSpec(spec);
        
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (!IsProductFound(product)) return NotFound();
        
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Post(Product product)
    {
        if (!IsProductValid(product)) return BadRequest();
        
        repository.Post(product);

        if (!await IsSaveSuccessful()) return BadRequest();
        
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, Product product)
    {
        if (!IsProductValid(product)) return BadRequest("Product is not valid");
        if (!IsProductExisting(id)) return NotFound("Product do not exists");
        if (!IsIdMatchingProduct(id, product)) return BadRequest("Id do not match the product");
        
        product.Id = id;
        repository.Put(product);
        
        if (!await IsSaveSuccessful()) return BadRequest("Product do not successfully saved");
        
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (!IsProductFound(product)) return NotFound();
        
        repository.Delete(product);
        
        if (!await IsSaveSuccessful()) return BadRequest();
        
        return NoContent();
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
    {
        var spec = new BrandSpec();
        return Ok(await repository.GetAllWithSpec(spec));
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
    {
        var spec = new TypeSpec();
        return Ok(await repository.GetAllWithSpec(spec));
    }

    // Validation Methods
    private static bool IsProductFound(Product? product)
    {
        return product is not null;
    }

    private bool IsProductExisting(int id)
    {
        return repository.Exists(id);
    }

    private static bool IsProductValid(Product product)
    {
        return product is not null && 
               !string.IsNullOrEmpty(product.Name) && 
               !string.IsNullOrEmpty(product.Description) &&
               !string.IsNullOrEmpty(product.PictureUrl) &&
               !string.IsNullOrEmpty(product.Type) &&
               !string.IsNullOrEmpty(product.Brand) &&
               product.Price > 0;
    }

    private static bool IsIdMatchingProduct(int id, Product product)
    {
        return product.Id == id;
    }

    private async Task<bool> IsSaveSuccessful()
    {
        return await repository.SaveChangesAsync();
    }
}
