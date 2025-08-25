using AutoMapper;
using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Data.Services;
using FeedUploader.Server.Automapper;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;
using FeedUploader.Server.Controllers;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json.Serialization;
ExcelPackage.License.SetNonCommercialPersonal("Heorhii Bruzha");

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMapper>(_ => AutoMapperConfig.GetConfiguration().CreateMapper());
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngularApp", policy =>
	{
		policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
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

app.MapControllers();

app.Run();
