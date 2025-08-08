using System.ComponentModel.DataAnnotations;

namespace FeedUploader.Server.DTOModels
{
	public class UserDTO : CreateUserDTO
	{
		[Required]
		public int Id { get; set; }
	}
	public class CreateUserDTO
	{
		[StringLength(20, MinimumLength = 2)]
		[Required]
		public string Name { get; set; }
		[StringLength(20, MinimumLength = 2)]
		[Required]
		public string Surname { get; set; }
		[EmailAddress]
		[StringLength(30, MinimumLength = 8)]
		[Required]
		public string Email { get; set; }
		[StringLength(50, MinimumLength = 8)]
		[Required]
		public string Password { get; set; }
		[StringLength(15, MinimumLength = 10)]
		[Required]
		public string ContactNumber { get; set; }
		[StringLength(20)]
		[Required]
		public string Role { get; set; }
	}
}
