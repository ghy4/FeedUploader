using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.MappingTools
{
    internal interface IMapper
    {
        public MapModel MapModel(string model, List<string> markets);
        public MapModel MapModel(List<string> models, string market);
    }
}
