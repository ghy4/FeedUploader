using AutoMapper;
using FeedUploader.Data.Models;
using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Server.DTOModels;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Mozilla;
using Attribute = FeedUploader.Data.Models.Attribute;

namespace FeedUploader.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AttributeController : Controller
	{
		private readonly IAttributeService _attributeService;
		private readonly IMapper _mapper;

		public AttributeController(IAttributeService attributeService, IMapper mapper)
		{
			_attributeService = attributeService;
			_mapper = mapper;
		}

		// GET: api/Attribute/Attributes
		[HttpGet("Attributes")]
		public async Task<ActionResult<IEnumerable<AttributeDTO>>> GetAttributes()
		{
			try
			{
				var attributes = await _attributeService.GetAll();
				var dtoList = _mapper.Map<IEnumerable<AttributeDTO>>(attributes);
				return Ok(dtoList);
			}
			catch
			{
				return StatusCode(500, "An error occurred while retrieving attributes");
			}
		}

		// GET: api/Attribute/Attribute/5
		[HttpGet("Attribute/{id:int}")]
		public async Task<ActionResult<AttributeDTO>> GetAttributeById(int id)
		{
			try
			{
				var attribute = await _attributeService.GetById(id);
				if (attribute == null)
					return NotFound();
				var dto = _mapper.Map<AttributeDTO>(attribute);
				return Ok(dto);
			}
			catch
			{
				return StatusCode(500, "An error occurred while retrieving the attribute");
			}
		}

		// POST: api/Attribute
		[HttpPost]
		public async Task<ActionResult> CreateAttribute(CreateAttributeDTO createDto)
		{
			if (!ModelState.IsValid)
				return BadRequest("Invalid attribute data");

			try
			{
				var attributeEntity = _mapper.Map<Attribute>(createDto);
				if (!await _attributeService.Create(attributeEntity))
					return StatusCode(500, "Could not add attribute");

				var dto = _mapper.Map<AttributeDTO>(attributeEntity);
				return CreatedAtAction(nameof(GetAttributeById), new { id = dto.Id }, dto);
			}
			catch
			{
				return StatusCode(500, "An error occurred while creating the attribute");
			}
		}

		// PUT: api/Attribute/5
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateAttribute(int id, UpdateAttributeDTO updateDto)
		{
			if (!ModelState.IsValid || id != updateDto.Id)
				return BadRequest("Invalid attribute data or ID mismatch");

			try
			{
				var existing = await _attributeService.GetById(id);
				if (existing == null)
					return NotFound();

				var entity = _mapper.Map<Attribute>(updateDto);
				if (!await _attributeService.Update(entity))
					return StatusCode(500, "Could not update attribute");

				return NoContent();
			}
			catch
			{
				return StatusCode(500, "An error occurred while updating the attribute");
			}
		}

		// DELETE: api/Attribute/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAttribute(int id)
		{
			try
			{
				var existing = await _attributeService.GetById(id);
				if (existing == null)
					return NotFound();

				await _attributeService.RemoveById(id);
				return NoContent();
			}
			catch
			{
				return StatusCode(500, "An error occurred while deleting the attribute");
			}
		}
	}
}
