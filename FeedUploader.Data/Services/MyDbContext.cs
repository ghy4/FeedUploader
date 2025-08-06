using Microsoft.EntityFrameworkCore;
using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services
{
	public class MyDbContext : DbContext
	{
		private readonly string _connectionstring;
		public DbSet<Product> Products { get; set; }
		public DbSet<User> Users { get; set; }
		public MyDbContext(string connectionstring)
		{
			_connectionstring = connectionstring;
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySQL(_connectionstring);
		}
	}
}
