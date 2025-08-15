using AutoMapper;
using FeedUploader.Data.Services.Interfaces;
using FeedUploader.Data.Services;
using FeedUploader.Server.Automapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAttributeService, AttributeService>();
builder.Services.AddScoped<IProductAttributeService, ProductAttributeService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped(_ => new MyDbContext(builder.Configuration.GetSection("ConnectionString").Value));
builder.Services.AddScoped<IMapper>(_ => AutoMapperConfig.GetConfiguration().CreateMapper());
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
