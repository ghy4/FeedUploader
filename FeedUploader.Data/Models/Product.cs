using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FeedUploader.Data.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; }
		public string Model { get; set; } = string.Empty;
		public string Manufacturer { get; set; } = string.Empty;
		public string Category { get; set; } = string.Empty;
		public decimal Price { get; set; } = 0.0m;
		public decimal SalePrice { get; set; } = 0.0m;
		public string Currency { get; set; } = "LEI";
		public int Quantity { get; set; } = 0;
		public int Warranty { get; set; } = 0; 
		public string MainImage { get; set; } = string.Empty;
		public string AdditionalImage1 { get; set; } = string.Empty;
		public string AdditionalImage2 { get; set; } = string.Empty;
		public string AdditionalImage3 { get; set; } = string.Empty;
		public string AdditionalImage4 { get; set; } = string.Empty;
		public string Type { get; set; } = "new";
		[NotMapped]
		public Dictionary<string, string>? Temps { get; set; }
	}
}
