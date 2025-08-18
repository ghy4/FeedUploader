using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.AttributeUtils
{
    public class AttributeModel
    {
        public string key { get; set; } = string.Empty;
        public List<string> values { get; set; } = new List<string>();
    }
}
