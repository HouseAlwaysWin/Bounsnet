using Bounsweet.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Services
{
    public class AuthService
    {
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpClientFactory _httpFactory;
        private readonly AntiforgeryOptions _antiforgeryOptions;
        private readonly ICachedService _cachedService;
        public AuthService(
                   IAntiforgery antiforgery,
                   IOptionsMonitor<AntiforgeryOptions> antiforgeryOptions,
                   IHttpClientFactory httpFactory,
                   ICachedService cachedService)
        {
            this._antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
            this._httpFactory = httpFactory ?? throw new ArgumentNullException(nameof(httpFactory));
            this._antiforgeryOptions = antiforgeryOptions.CurrentValue ?? throw new ArgumentNullException(nameof(antiforgeryOptions));
            this._cachedService = cachedService ?? throw new ArgumentNullException(nameof(cachedService));
        }

        /// <summary>
        /// Generated Token
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="issuer"></param>
        /// <param name="claims"></param>
        /// <param name="expiresTime">過期時間(分鐘)</param>
        /// <returns></returns>
        public string GenerateJwtToken(string tokenKey, string issuer, List<Claim> claims, int expiresTime = 5)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresTime),
                SigningCredentials = creds,
                Issuer = issuer
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenstring = tokenHandler.WriteToken(token);
            return tokenstring;
        }

        /// <summary>
        /// Generated Refresh Token
        /// </summary>
        /// <param name="ipAddress">Ip位址</param>
        /// <param name="expireTime">過期時間(預設為七天10080分鐘)</param>
        /// <returns></returns>
        public RefreshToken GenerateRefreshToken(string ipAddress, int expireTime = 10080)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            var createdTime = DateTime.UtcNow;
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                ExpiredTime = createdTime.AddMinutes(expireTime),
                IssuedTime = createdTime,
            };

            return refreshToken;
        }

        /// <summary>
        /// Generated Password Hash
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string GeneratePasswordHash(string password, string salt, int iterationCount = 10000, int numBytesRequested = 128)
        {

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount,
                numBytesRequested));
            return hashed;
        }

        /// <summary>
        /// Validated Password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public bool ValidatedPassword(string password, string passwordHash, string salt)
        {
            var getPasswordHash = GeneratePasswordHash(password, salt);
            if (getPasswordHash == passwordHash)
            {
                return true;
            }
            return false;
        }

    }
}
