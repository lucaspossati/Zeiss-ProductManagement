using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Zeiss.ProductManagement.Api.Contracts;
using Zeiss.ProductManagement.Application.Products;
using Zeiss.ProductManagement.Application.Products.Commands;
using Zeiss.ProductManagement.Application.Products.Handlers;
using Zeiss.ProductManagement.Application.Products.Queries;

namespace Zeiss.ProductManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly ICreateProductHandler _createProductHandler;
    private readonly IUpdateProductHandler _updateProductHandler;
    private readonly IDeleteProductHandler _deleteProductHandler;
    private readonly IAddStockHandler _addStockHandler;
    private readonly IDecrementStockHandler _decrementStockHandler;
    private readonly IGetAllProductsHandler _getAllProductsHandler;
    private readonly IGetProductByIdHandler _getProductByIdHandler;
    private readonly ISearchProductsByNameHandler _searchProductsByNameHandler;
    private readonly IGetProductsByStockLevelHandler _getProductsByStockLevelHandler;
    private readonly IValidator<AddStockCommand> _addStockValidator;
    private readonly IValidator<DecrementStockCommand> _decrementStockValidator;

    public ProductsController(
        ICreateProductHandler createProductHandler,
        IUpdateProductHandler updateProductHandler,
        IDeleteProductHandler deleteProductHandler,
        IAddStockHandler addStockHandler,
        IDecrementStockHandler decrementStockHandler,
        IGetAllProductsHandler getAllProductsHandler,
        IGetProductByIdHandler getProductByIdHandler,
        ISearchProductsByNameHandler searchProductsByNameHandler,
        IGetProductsByStockLevelHandler getProductsByStockLevelHandler,
        IValidator<AddStockCommand> addStockValidator,
        IValidator<DecrementStockCommand> decrementStockValidator)
    {
        _createProductHandler = createProductHandler;
        _updateProductHandler = updateProductHandler;
        _deleteProductHandler = deleteProductHandler;
        _addStockHandler = addStockHandler;
        _decrementStockHandler = decrementStockHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _getProductByIdHandler = getProductByIdHandler;
        _searchProductsByNameHandler = searchProductsByNameHandler;
        _getProductsByStockLevelHandler = getProductsByStockLevelHandler;
        _addStockValidator = addStockValidator;
        _decrementStockValidator = decrementStockValidator;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<PaginatedResult<ProductDto>>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? minStock = null,
        [FromQuery] int? maxStock = null)
    {
        var result = await _getAllProductsHandler.Handle(new GetAllProductsQuery(page, pageSize, search, minStock, maxStock), HttpContext.RequestAborted);
        return Ok(BaseResponse<PaginatedResult<ProductDto>>.Ok(result, "Products retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<ProductDto>>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await _createProductHandler.Handle(command, HttpContext.RequestAborted);
        return CreatedAtAction(nameof(GetProductByIdentifier), new { id = result.ProductIdentifier }, BaseResponse<ProductDto>.Ok(result, "Product created successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<ProductDto>>> GetProductByIdentifier([FromRoute] int id)
    {
        var result = await _getProductByIdHandler.Handle(new GetProductByIdQuery(id), HttpContext.RequestAborted);
        return Ok(BaseResponse<ProductDto>.Ok(result, "Product retrieved successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<ProductDto>>> UpdateProduct([FromRoute] int id, [FromBody] UpdateProductCommand command)
    {
        var request = command with { ProductIdentifier = id };
        var result = await _updateProductHandler.Handle(request, HttpContext.RequestAborted);
        return Ok(BaseResponse<ProductDto>.Ok(result, "Product updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct([FromRoute] int id)
    {
        await _deleteProductHandler.Handle(new DeleteProductCommand(id), HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("{id:int}/decrement-stock/{quantity:int}")]
    public async Task<ActionResult<BaseResponse<ProductDto>>> DecrementStock([FromRoute] int id, [FromRoute] int quantity)
    {
        var command = new DecrementStockCommand(id, quantity);
        var validation = await _decrementStockValidator.ValidateAsync(command, HttpContext.RequestAborted);
        if (!validation.IsValid)
        {
            return BadRequest(BaseResponse<ProductDto>.Fail(
                "One or more validation errors occurred.",
                validation.Errors.Select(error => new ApiValidationError(
                    "Product",
                    error.PropertyName,
                    error.ErrorCode,
                    error.ErrorMessage)).ToArray()));
        }

        var result = await _decrementStockHandler.Handle(command, HttpContext.RequestAborted);
        return Ok(BaseResponse<ProductDto>.Ok(result, "Stock decremented successfully."));
    }

    [HttpPost("{id:int}/add-to-stock/{quantity:int}")]
    public async Task<ActionResult<BaseResponse<ProductDto>>> AddToStock([FromRoute] int id, [FromRoute] int quantity)
    {
        var command = new AddStockCommand(id, quantity);
        var validation = await _addStockValidator.ValidateAsync(command, HttpContext.RequestAborted);
        if (!validation.IsValid)
        {
            return BadRequest(BaseResponse<ProductDto>.Fail(
                "One or more validation errors occurred test.",
                validation.Errors.Select(error => new ApiValidationError(
                    "Product",
                    error.PropertyName,
                    error.ErrorCode,
                    error.ErrorMessage)).ToArray()));
        }

        var result = await _addStockHandler.Handle(command, HttpContext.RequestAborted);
        return Ok(BaseResponse<ProductDto>.Ok(result, "Stock added successfully."));
    }

    [HttpGet("search")]
    public async Task<ActionResult<BaseResponse<PaginatedResult<ProductDto>>>> SearchProducts([FromQuery] string name, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _searchProductsByNameHandler.Handle(new SearchProductsByNameQuery(name, page, pageSize), HttpContext.RequestAborted);
        return Ok(BaseResponse<PaginatedResult<ProductDto>>.Ok(result, "Products matching the search criteria."));
    }

    [HttpGet("stock-level")]
    public async Task<ActionResult<BaseResponse<PaginatedResult<ProductDto>>>> GetProductsByStockLevel(
        [FromQuery] int? min,
        [FromQuery] int? max,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _getProductsByStockLevelHandler.Handle(new GetProductsByStockLevelQuery(min, max, page, pageSize), HttpContext.RequestAborted);
        return Ok(BaseResponse<PaginatedResult<ProductDto>>.Ok(result, "Products filtered by stock level."));
    }
}
