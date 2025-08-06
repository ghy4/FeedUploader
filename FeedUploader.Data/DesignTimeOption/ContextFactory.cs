using FeedUploader.Data.Services;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace FeedUploader.Data.DesignTimeOption
{
	internal class ContextFactory : IDesignTimeDbContextFactory<MyDbContext>
	{
		public MyDbContext CreateDbContext(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddUserSecrets(Assembly.GetExecutingAssembly());
			var configuration = builder.Build();
			var url = configuration.GetSection("ConnectionString").Value;

			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentException("Connection string is not configured in user secrets.", nameof(url));
			}
			return new MyDbContext(url);
		}
	}
}
