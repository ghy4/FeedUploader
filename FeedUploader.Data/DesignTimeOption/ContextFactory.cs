using FeedUploader.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FeedUploader.Data.DesignTimeOption
{
	public class ContextFactory : IDesignTimeDbContextFactory<MyDbContext>
	{
		public MyDbContext CreateDbContext(string[] args)
		{
			var builder = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddUserSecrets(Assembly.GetExecutingAssembly());
			var configuration = builder.Build();
			var connectionString = configuration.GetSection("ConnectionString").Value;

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentException("Connection string is not configured in user secrets.", nameof(connectionString));
			}

			var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
			optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

			return new MyDbContext(optionsBuilder.Options);
		}
	}
}
