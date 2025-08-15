using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedUploader.Data.Models
{
	public class ProductAttribute
	{
		[Key]
		public int Id { get; set; }
		public int ProductId { get; set; }
		public Product? Product { get; set; }
		public int AttributeId { get; set; }
		public Attribute? Attribute { get; set; }
		public string Value { get; set; } = string.Empty;
		public bool IsExtractedByAI { get; set; }
	}
}
