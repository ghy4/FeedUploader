using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.MappingTools
{
    public class MapModel
    {
        [Key]
        public string Market { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
    }
}
