using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedUploader.Data.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Model { get; set; } = string.Empty;
		public string Manufacturer { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public decimal Price { get; set; } = 0.0m;
		public decimal SalePrice { get; set; } = 0.0m;
		public string Currency { get; set; } = "LEI";
		public int Quantity { get; set; } = 0;
		public int? Warranty { get; set; } = null;
		public string MainImage { get; set; } = string.Empty;
		public string AdditionalImage1 { get; set; } = string.Empty;
		public string AdditionalImage2 { get; set; } = string.Empty;
		public string AdditionalImage3 { get; set; } = string.Empty;
		public string AdditionalImage4 { get; set; } = string.Empty;
		public string Type { get; set; } = "new";
		public int UserId { get; set; }
		public User? User { get; set; }
		public List<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
		[NotMapped]
		public Dictionary<string, string>? ExtractedAttributes { get; set; }
	}
}
