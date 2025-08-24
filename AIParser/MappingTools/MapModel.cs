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
        public int Id { get; set; }                // identificator unic al mapării
        public string SourceField { get; set; } = string.Empty; // ex: "Cod produs", "Brand"
        public string DestinationField { get; set; } = string.Empty; // ex: "Model", "Manufacturer"
        public string Market { get; set; } = string.Empty;   // ex: "Contakt", "Interlink", "Emag", dat de utilizator
        public int? UserId { get; set; }           
    }
}
