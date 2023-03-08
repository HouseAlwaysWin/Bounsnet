using Bounsweet.Models;

namespace Bounsweet.Services
{
    public interface IRefreshTokenService
    {
        Task<bool> AddRefreshTokenAsync(RefreshToken token);
        Task<List<RefreshToken>> GetAllRefreshTokensAsync(string id, string userName);
        Task<RefreshToken> GetRefreshToken(string id, string userName);
        Task RemoveRefreshTokenAsync(RefreshToken token);
        Task RemoveRefreshTokenAsync(string id, string userName);
        Task RevokeRefreshTokenAsync(RefreshToken token);
    }
}