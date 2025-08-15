using Microsoft.EntityFrameworkCore;
using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services
{
	public class MyDbContext : DbContext
	{
		private readonly string _connectionstring;
		public DbSet<Product> Products { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Models.Attribute> Attributes { get; set; }
		public DbSet<ProductAttribute> ProductAttributes { get; set; }
		public MyDbContext(string connectionstring)
		{
			_connectionstring = connectionstring;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL(_connectionstring);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Product>()
				.HasOne(p => p.User)
				.WithMany(u => u.Products)
				.HasForeignKey(p => p.UserId);

			modelBuilder.Entity<ProductAttribute>()
				.HasOne(pa => pa.Product)
				.WithMany(p => p.Attributes)
				.HasForeignKey(pa => pa.ProductId);

			modelBuilder.Entity<ProductAttribute>()
				.HasOne(pa => pa.Attribute)
				.WithMany()
				.HasForeignKey(pa => pa.AttributeId);
		}
	}
}
