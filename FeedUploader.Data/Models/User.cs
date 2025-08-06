using System.ComponentModel.DataAnnotations;

namespace FeedUploader.Data.Models
{
    public class User
    {
		[Key]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Surname { get; set; } = string.Empty;
		public string FullName => $"{Name} {Surname}";
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string ContactNumber { get; set; } = string.Empty;
		public string Role { get; set; } = "User";
	}
}
