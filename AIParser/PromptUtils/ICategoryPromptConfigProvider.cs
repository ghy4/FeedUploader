using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIParser.PromptUtils
{
    public interface ICategoryPromptConfigProvider
    {
        Task<List<PromptCategoryConfig>> GetAllAsync();
    }
}
