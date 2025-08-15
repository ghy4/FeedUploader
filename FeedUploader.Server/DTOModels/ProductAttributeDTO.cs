using FeedUploader.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace FeedUploader.Server.DTOModels
{
	public class ProductAttributeDTO : UpdateProductAttributeDTO
	{
		public Product? Product { get; set; }
		public Data.Models.Attribute? Attribute { get; set; }
	}

	public class CreateProductAttributeDTO
	{
		[Required]
		[Range(1, int.MaxValue)]
		public int ProductId { get; set; }

		[Required]
		[Range(1, int.MaxValue)]
		public int AttributeId { get; set; }

		[Required]
		[StringLength(255)]
		public string Value { get; set; } = string.Empty;

		public bool IsExtractedByAI { get; set; } = false;
	}

	public class UpdateProductAttributeDTO : CreateProductAttributeDTO
	{
		[Required]
		[Range(1, int.MaxValue)]
		public int Id { get; set; }
	}
}
