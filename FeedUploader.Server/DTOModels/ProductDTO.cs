using System.ComponentModel.DataAnnotations;
namespace FeedUploader.Server.DTOModels
{
	public class ProductDTO : UpdateProductDTO
	{
		public List<ProductAttributeDTO> Attributes { get; set; } = new();
		public Dictionary<string, string>? ExtractedAttributes { get; set; }
	}
	public class CreateProductDTO
	{
		[Required]
		[StringLength(30, MinimumLength = 2)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[StringLength(500, MinimumLength = 10)]
		public string Description { get; set; } = string.Empty;

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Model { get; set; } = string.Empty;

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Manufacturer { get; set; } = string.Empty;

		[Required]
		[StringLength(50, MinimumLength = 1)]
		public string Category { get; set; } = string.Empty;

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal Price { get; set; }

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal SalePrice { get; set; }

		[Required]
		[StringLength(15)]
		[RegularExpression(@"^[A-Z]{3}$")]
		public string Currency { get; set; } = "RON";

		[Required]
		[Range(0, int.MaxValue)]
		public int Quantity { get; set; }

		[Range(0, 120)]
		public int Warranty { get; set; }

		[Required]
		[StringLength(4096)]
		[Url]
		public string MainImage { get; set; } = string.Empty;
		[Required]
		[StringLength(20)]
		[RegularExpression(@"^(new|used|refurbished)$")]
		public string Type { get; set; } = "new";
	}
	public class UpdateProductDTO : CreateProductDTO
	{
		[Required]
		[Range(0, int.MaxValue)]
		public int Id { get; set; }
		[Range(0, 120)]
		public int Warranty { get; set; }
		[StringLength(4096)]
		[Url]
		public string? AdditionalImage1 { get; set; }
		[StringLength(4096)]
		[Url]
		public string? AdditionalImage2 { get; set; }
		[StringLength(4096)]
		[Url]
		public string? AdditionalImage3 { get; set; }
		[StringLength(4096)]
		[Url]
		public string? AdditionalImage4 { get; set; }
	}
}
