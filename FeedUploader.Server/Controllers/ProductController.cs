using AutoMapper;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Server.DTOModels;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Mozilla;

namespace FeedUploader.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductController : Controller
	{
		private readonly IProductService _productService;
		private readonly IMapper _mapper;
		public ProductController(IProductService productService, IMapper mapper)
		{
			_productService = productService;
			_mapper = mapper;
		}
		[Route("Products")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
		{
			try
			{
				var products = _mapper.Map<IEnumerable<ProductDTO>>(await _productService.GetAll());
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving products");
			}
		}
		[Route("Product/{id:int}")]
		[HttpGet]
		public async Task<ActionResult<ProductDTO>> GetProductById(int id)
		{
			try
			{
				var product = await _productService.GetById(id);
				if (product == null)
					return NotFound();
				var dto = _mapper.Map<ProductDTO>(product);
				return Ok(dto);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while retrieving the product");
			}
		}
		[HttpPost]
		public async Task<ActionResult> PostProduct(CreateProductDTO product)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid product data");

			try
			{
				var productEntity = _mapper.Map<Product>(product);
				if (!await _productService.Create(productEntity))
					return StatusCode(500, "Could not add product");
				var productDto = _mapper.Map<ProductDTO>(productEntity);
				return CreatedAtAction(nameof(GetProductById), new { id = productDto.Id }, productDto);
			}
			catch
			{
				return StatusCode(500, "An error occurred while creating the product");
			}
		}
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateProduct(int id, UpdateProductDTO product)
		{
			if (!ModelState.IsValid || id != product.Id)
				return BadRequest("Invalid product data or ID mismatch");
			try
			{
				var existingProduct = await _productService.GetById(id);
				if (existingProduct == null)
					return NotFound();
				var productEntity = _mapper.Map<Product>(product);
				if (!await _productService.Update(productEntity))
					return StatusCode(500, "Could not update product");
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while updating the product");
			}
		}
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			try
			{
				var product = await _productService.GetById(id);
				if (product == null)
					return NotFound();
				await _productService.RemoveById(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error occurred while deleting the product");
			}
		}
	}
}
