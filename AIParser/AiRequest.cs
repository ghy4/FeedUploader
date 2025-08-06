using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser
{
    public class AiRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public int MaxTokens { get; set; } = 100;
    }
}
