using AutoMapper;
using FeedUploader.Data.Models;
using FeedUploader.Server.DTOModels;

namespace FeedUploader.Server.Automapper
{
	public class AutoMapperConfig
	{

		public static MapperConfiguration GetConfiguration()
		{
			return new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<User, UserDTO>();
				cfg.CreateMap<CreateUserDTO, User>();
				cfg.CreateMap<Product, ProductDTO>();
				cfg.CreateMap<CreateProductDTO, Product>();
				cfg.CreateMap<UpdateProductDTO, Product>();
			}, new LoggerFactory());
		}
	}
}