using StackExchange.Redis;

namespace Bounsnet.Services
{
    public interface IRedisService
    {
        IDatabase RedisClient { get; }

        Task AddHashJsonValue<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None);
        Task AddOrUpdateAllHashJsonValueAsync<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None);
        Task AddOrUpdateHashJsonValueAsync<T>(string key, string childKey, T data, CommandFlags flags = CommandFlags.None);
        Task<Dictionary<string, T>> GetAllHashJsonValueAsync<T>(string key, CommandFlags flags = CommandFlags.None);
        Dictionary<string, string> GetHashAllJsonValue(string key, CommandFlags flags = CommandFlags.None);
        T GetHashJsonValue<T>(string key, string childKey, CommandFlags flags = CommandFlags.None);
        Task<T> GetHashJsonValueAsync<T>(string key, string childKey, CommandFlags flags = CommandFlags.None);
        void SetRedisConfig(string connString);
    }
}