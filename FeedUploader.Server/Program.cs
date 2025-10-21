using AutoMapper;
using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Data.Services;
using FeedUploader.Server.Automapper;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using FeedUploader.Server.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using FeedUploader.Server.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add file logger provider (writes to /logs folder)
var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddProvider(new FileLoggerProvider(logDir));

builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		// Return clean JSON arrays (avoid $id/$values) while ignoring cycles
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.MaxDepth = 64; // Increase depth limit if needed
	});
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddScoped(_ => new MyDbContext(builder.Configuration.GetSection("ConnectionString").Value));
builder.Services.AddDbContext<MyDbContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("ConnectionString");
	if (string.IsNullOrEmpty(connectionString))
	{
		throw new ArgumentException("Connection string 'DefaultConnection' not found.", nameof(connectionString));
	}
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAttributeService, AttributeService>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();
builder.Services.AddScoped<IFeedParsingAdapter, FeedUploader.Server.Services.Adapters.AiParserFeedParsingAdapter>();
builder.Services.AddScoped<IExportAdapter, FeedUploader.Server.Services.Adapters.AiParserEmagExportAdapter>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMapper>(_ => AutoMapperConfig.GetConfiguration().CreateMapper());
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngularApp", policy =>
	{
		policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://127.0.0.1:4200", "https://127.0.0.1:4200")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "FeedUploader API V1");
	});
}

//app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();

// Basic request logging middleware
app.Use(async (context, next) =>
{
	var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLogger");
	logger.LogInformation("{Method} {Path}", context.Request.Method, context.Request.Path);
	try
	{
		await next();
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
		throw;
	}
});

app.MapControllers();

app.Run();
