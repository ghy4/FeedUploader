using System.ComponentModel.DataAnnotations;

namespace FeedUploader.Server.DTOModels
{
	public class AttributeDTO : UpdateAttributeDTO
	{
		public List<string>? AllowedValues { get; set; }
	}
	public class CreateAttributeDTO
	{
		[Required]
		[StringLength(50, MinimumLength = 2)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[StringLength(30, MinimumLength = 1)]
		public string Code { get; set; } = string.Empty;

		[Required]
		public bool IsRequired { get; set; }

		[Required]
		public bool IsRestricted { get; set; }

		[StringLength(20)]
		public string? Unit { get; set; }
	}

	public class UpdateAttributeDTO : CreateAttributeDTO
	{
		[Required]
		[Range(0, int.MaxValue)]
		public int Id { get; set; }
	}
}
