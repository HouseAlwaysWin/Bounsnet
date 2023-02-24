﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Models
{
    public class JwtAuthOptions : JwtBearerOptions
    {
        public bool SaveJwtInCookie { get; set; }
    }
}
