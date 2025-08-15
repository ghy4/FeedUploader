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
	public class ProductAttributeController : Controller
	{
		private readonly IProductAttributeService _service;
		private readonly IMapper _mapper;

		public ProductAttributeController(IProductAttributeService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}
		// GET: api/ProductAttribute/{id}
		[HttpGet("{id:int}")]
		public async Task<ActionResult<ProductAttributeDTO>> GetById(int id)
		{
			try
			{
				var attribute = await _service.GetById(id);
				if (attribute == null)
					return NotFound();

				var dto = _mapper.Map<ProductAttributeDTO>(attribute);
				return Ok(dto);
			}
			catch
			{
				return StatusCode(500, "An error occurred while retrieving the product attribute");
			}
		}

		// POST: api/ProductAttribute
		[HttpPost]
		public async Task<ActionResult> Create(CreateProductAttributeDTO createDto)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid data");

			try
			{
				var entity = _mapper.Map<ProductAttribute>(createDto);
				if (!await _service.Create(entity))
					return StatusCode(500, "Could not create product attribute");

				var dto = _mapper.Map<ProductAttributeDTO>(entity);
				return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
			}
			catch
			{
				return StatusCode(500, "An error occurred while creating the product attribute");
			}
		}

		// PUT: api/ProductAttribute/{id}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, UpdateProductAttributeDTO updateDto)
		{
			if (!ModelState.IsValid || id != updateDto.Id)
				return BadRequest("Invalid data or ID mismatch");

			try
			{
				var existing = await _service.GetById(id);
				if (existing == null)
					return NotFound();

				var entity = _mapper.Map<ProductAttribute>(updateDto);
				if (!await _service.Update(entity))
					return StatusCode(500, "Could not update product attribute");

				return NoContent();
			}
			catch
			{
				return StatusCode(500, "An error occurred while updating the product attribute");
			}
		}

		// DELETE: api/ProductAttribute/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				var existing = await _service.GetById(id);
				if (existing == null)
					return NotFound();

				await _service.RemoveById(id);
				return NoContent();
			}
			catch
			{
				return StatusCode(500, "An error occurred while deleting the product attribute");
			}
		}
	}
}
