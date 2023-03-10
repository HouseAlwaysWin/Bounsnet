using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Models
{
    public class RefreshTokenOptions
    {
        public RefreshTokenProvider StoreProvider { get; set; } = RefreshTokenProvider.MemoryCache;
        public string RedisConnection { get; set; }
    }

    public enum RefreshTokenProvider
    {
        Redis,
        MemoryCache,
        SqlServer
    }
}
