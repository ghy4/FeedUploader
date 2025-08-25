using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Server.DTOModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using FeedUploader.Data.Models;

namespace FeedUploader.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FeedController : ControllerBase
	{
		private readonly IFeedService _feedService;
		private readonly IMapper _mapper;

		public FeedController(IFeedService feedService, IMapper imapper)
		{
			_feedService = feedService;
			_mapper = imapper;
		}

		/// <summary>
		/// Uploads a CSV feed and imports products with attributes.
		/// </summary>
		[HttpPost("upload")]
		public async Task<ActionResult<ICollection<ProductDTO>>> UploadFeed([FromForm] FeedUploadRequest request)
		{
			if (request.File == null || request.File.Length == 0)
				return BadRequest("No file uploaded");

			try
			{
				var products = await _feedService.UploadFeedAsync(request.File, request.UserId);

				if (products == null)
					return StatusCode(500, "Feed service returned null products.");

				var result = _mapper.Map<IEnumerable<ProductDTO>>(products);

				return Ok(result);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"UploadFeed error: {ex}");
				return StatusCode(500, $"Failed to upload feed: {ex.Message}");
			}
		}

		/// <summary>
		/// Generates an Excel file for given product IDs.
		/// </summary>
		[HttpPost("export")]
		public async Task<IActionResult> GenerateExcel([FromBody] ICollection<int> productIds)
		{
			if (productIds == null || !productIds.Any())
				return BadRequest("No product IDs provided");
			try
			{
				var fileBytes = await _feedService.GenerateExcelAsync(productIds);
				return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products.xlsx");
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Failed to generate Excel: {ex.Message}");
			}
		}

		/// <summary>
		/// Clears the database (dangerous operation).
		/// </summary>
		[HttpDelete("clear")]
		public async Task<IActionResult> ClearDatabase()
		{
			try
			{
				var success = await _feedService.ClearDatabaseAsync();
				if (!success) return StatusCode(500, "Failed to clear database");
				return NoContent();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Failed to clear database: {ex.Message}");
			}
		}
			   // Remove Index action for API controller
	}

	public class FeedUploadRequest
	{
		public IFormFile File { get; set; }
		public int UserId { get; set; }
	}
}
