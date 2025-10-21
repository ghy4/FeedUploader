using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.DataUtils
{
    public class RawFeedData //clasa care tine datele extrase din feed, inainte de procesare
    {
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }

}
