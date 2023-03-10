using Bounsnet.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ICachedService _cachedService;
        private readonly IRedisService _redisService;
        private RefreshTokenOptions _options;

        public RefreshTokenService(IRedisService redisService, ICachedService cachedService)
        {
            this._cachedService = cachedService;
            this._redisService = redisService;
            _options = new RefreshTokenOptions();
            _redisService.SetRedisConfig(_options.RedisConnection);
        }

        public void SetRefreshTokenOptions(RefreshTokenOptions options)
        {
            this._options = options;
        }

        public async Task<List<RefreshToken>> GetAllRefreshTokensAsync(string id, string userName)
        {
            var key = $"{userName}_RefreshTokens";
            List<RefreshToken> tokens = new List<RefreshToken>();

            switch (_options.StoreProvider)
            {
                case RefreshTokenProvider.Redis:
                    var datas = await _redisService.GetAllHashJsonValueAsync<RefreshToken>(key);
                    tokens = datas.Values.ToList();
                    break;
                default:
                    tokens = await _cachedService.GetAndSetJsonDataAsync(key, new List<RefreshToken>());
                    break;
            }

            return tokens;
        }

        public async Task<RefreshToken> GetRefreshToken(string id, string userName)
        {
            var key = $"{userName}_RefreshTokens";
            RefreshToken token = new RefreshToken();
            switch (_options.StoreProvider)
            {
                case RefreshTokenProvider.Redis:
                    token = await _redisService.GetHashJsonValueAsync<RefreshToken>(key, id);
                    break;
                default:
                    var tokens = await GetAllRefreshTokensAsync(id, userName);
                    token = tokens.FirstOrDefault(t => t.UserName == userName && t.ClientId == id);
                    break;
            }
            return token;
        }

        public async Task<bool> AddRefreshTokenAsync(RefreshToken token)
        {
            var key = $"{token.UserName}_RefreshTokens";
            List<RefreshToken> tokens = new List<RefreshToken>();

            var result = false;
            switch (_options.StoreProvider)
            {
                case RefreshTokenProvider.Redis:
                    var datas = await _redisService.GetAllHashJsonValueAsync<RefreshToken>(key);
                    await _redisService.AddOrUpdateHashJsonValueAsync(key, token.Token, token);
                    result = true;
                    break;

                default:
                    tokens = await GetAllRefreshTokensAsync(token.ClientId, token.UserName);
                    var existingToken = tokens.FirstOrDefault(t => t.UserName == token.UserName && t.ClientId == token.ClientId);
                    if (existingToken != null)
                    {
                        tokens.Remove(existingToken);
                    }
                    tokens.Add(token);
                    result = await _cachedService.SetJsonDataAsync(key, tokens);
                    break;
            }

            return result;
        }

        public async Task RemoveRefreshTokenAsync(string id, string userName)
        {
            var key = $"{userName}_RefreshTokens";
            switch (_options.StoreProvider)
            {
                case RefreshTokenProvider.Redis:
                    await _redisService.RedisClient.HashDeleteAsync(key, id);
                    break;
                default:
                    var tokens = await GetAllRefreshTokensAsync(id, userName);
                    var token = tokens.FirstOrDefault(t => t.ClientId == id && t.UserName == userName);
                    tokens.Remove(token);
                    break;
            }
        }

        public async Task RemoveRefreshTokenAsync(RefreshToken token)
        {
            await RemoveRefreshTokenAsync(token.ClientId, token.UserName);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            var key = $"{token.UserName}_RefreshTokens";

            switch (_options.StoreProvider)
            {
                case RefreshTokenProvider.Redis:
                    var currToken = await _redisService.GetHashJsonValueAsync<RefreshToken>(key, token.Token);
                    currToken.Active = false;
                    await _redisService.AddOrUpdateHashJsonValueAsync(key, token.Token, currToken);
                    break;
                default:
                    var tokens = await GetAllRefreshTokensAsync(token.ClientId, token.UserName);
                    var currentToken = tokens.FirstOrDefault(t => t.ClientId == token.ClientId && t.UserName == token.UserName);
                    currentToken.Active = false;
                    await AddRefreshTokenAsync(currentToken);
                    break;
            }
        }

    }
}
