using Bounsweet.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsweet.Services
{
    public class RefreshTokenService
    {
        private readonly ICachedService _cachedService;

        public RefreshTokenService(ICachedService cachedService)
        {
            this._cachedService = cachedService;
        }

        public async Task<List<RefreshToken>> GetAllRefreshTokensAsync(string id, string userName)
        {
            var key = $"{userName}_RefreshTokens";
            var tokens = await _cachedService.GetAndSetJsonDataAsync(key, new List<RefreshToken>());
            return tokens;
        }

        public async Task<RefreshToken> GetRefreshToken(string id, string userName)
        {
            var tokens = await GetAllRefreshTokensAsync(id, userName);
            var token = tokens.FirstOrDefault(t => t.UserName == userName && t.ClientId == id);
            return token;
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken token)
        {
            var key = $"{token.UserName}_RefreshTokens";

            var tokens = await GetAllRefreshTokensAsync(token.ClientId, token.UserName);

            if (tokens.Count > 10)
            {

            }

            var existingToken = tokens.FirstOrDefault(t => t.UserName == token.UserName && t.ClientId == token.ClientId);
            if (existingToken != null)
            {
                tokens.Remove(existingToken);
            }
            tokens.Add(token);
            var result = await _cachedService.SetJsonDataAsync(key, tokens);

            return result;
        }

        public async Task RemoveRefreshTokenAsync(string id, string userName)
        {
            var tokens = await GetAllRefreshTokensAsync(id, userName);
            var token = tokens.FirstOrDefault(t => t.ClientId == id && t.UserName == userName);
            tokens.Remove(token);
        }

        public async Task RemoveRefreshTokenAsync(RefreshToken token)
        {
            var tokens = await GetAllRefreshTokensAsync(token.ClientId, token.UserName);
            tokens.Remove(token);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            var tokens = await GetAllRefreshTokensAsync(token.ClientId, token.UserName);
            var currentToken = tokens.FirstOrDefault(t => t.ClientId == token.ClientId && t.UserName == token.UserName);
            currentToken.Active = false;
            await AddRefreshTokenAsync(currentToken);
        }

    }
}
