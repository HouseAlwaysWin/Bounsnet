using Bounsnet.Middlewares;
using Bounsnet.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Extensions
{
    public static class JwtTokenAuthExtensions
    {
        public static AuthenticationBuilder UseJwtAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<JwtAuthOptions> options)
        {
            builder.AddScheme<JwtAuthOptions, JwtAuthHandler<JwtAuthOptions>>(authenticationScheme, options);

            return builder;
        }

        public static AuthenticationBuilder UseJwtAuth(this AuthenticationBuilder builder, Action<JwtAuthOptions> options)
        {
            builder.AddScheme<JwtAuthOptions, JwtAuthHandler<JwtAuthOptions>>(JwtBearerDefaults.AuthenticationScheme, options);

            return builder;
        }
    }
}
