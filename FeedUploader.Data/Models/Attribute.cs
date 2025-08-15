using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedUploader.Data.Models
{
	public class Attribute
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Code { get; set; } = string.Empty;
		public bool IsRequired { get; set; }
		public bool IsRestricted { get; set; }
		public string? Unit { get; set; }
		[NotMapped]
		public List<string>? AllowedValues { get; set; }
	}
}
