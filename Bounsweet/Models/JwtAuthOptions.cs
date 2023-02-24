using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Models
{
    public class JwtAuthOptions : AuthenticationSchemeOptions
    {
        public string AuthenticationScheme { get; set; }
    }
}
