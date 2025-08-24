using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.MappingTools
{
    public interface IMapperService
    {
        Task<MapModel?> FindMapAsync(string sourceField, string market, int? userId = null);

        Task<MapModel> SaveMapAsync(string sourceField, string destinationField, string market, int? userId = null);

        Task<List<MapModel>> GetMapsForMarketAsync(string market, int? userId = null);

        Task<List<List<MapModel>>> GetMapsForUserAsync(int userId);
    }
}
