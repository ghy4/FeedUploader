namespace AIParser.MappingTools
{

    public class MapperService : IMapperService // de pus si adaptat la services pe urma
    {
        private readonly ApplicationDbContext _db;

        public MapperService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<List<MapModel>>> GetMapsForUserAsync(int userId)
        {
            return await _db.MapModels
                .Where(m => m.UserId == userId)
                .GroupBy(m => m.Market)  
                .Select(g => g.ToList())
                .ToListAsync();
        }
        public async Task<MapModel?> FindMapAsync(string sourceField, string market, int? userId = null)
        {
            return await _db.MapModels
                .FirstOrDefaultAsync(m => m.SourceField == sourceField
                                          && m.Market == market
                                          && (userId == null || m.UserId == userId));
        }

        public async Task<MapModel> SaveMapAsync(string sourceField, string destinationField, string market,
            int? userId = null)
        {
            var map = await FindMapAsync(sourceField, market, userId);

            if (map == null)
            {
                map = new MapModel
                {
                    SourceField = sourceField,
                    DestinationField = destinationField,
                    Market = market,
                    UserId = userId
                };
                _db.MapModels.Add(map);
            }
            else
            {
                map.DestinationField = destinationField;
            }

            await _db.SaveChangesAsync();
            return map;
        }

        public async Task<List<MapModel>> GetMapsForMarketAsync(string market, int? userId = null)
        {
            return await _db.MapModels
                .Where(m => m.Market == market && (userId == null || m.UserId == userId))
                .ToListAsync();
        }
    }
}