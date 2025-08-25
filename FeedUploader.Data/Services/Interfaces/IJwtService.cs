using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedUploader.Data.Models;

namespace FeedUploader.Data.Services.Interfaces
{
	public interface IJwtService
	{
		string GenerateToken(User user);
	}
}
