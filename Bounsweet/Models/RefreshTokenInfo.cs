using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Models
{
    public class RefreshTokenInfo
    {
        public long ClientKeyId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ClientName { get; set; }
        public bool Active { get; set; }
        public int LifeTime { get; set; }
        public string AllowedOrigin { get; set; }
    }
}
