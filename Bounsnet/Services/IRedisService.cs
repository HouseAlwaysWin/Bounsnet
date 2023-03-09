using StackExchange.Redis;

namespace Bounsnet.Services
{
    public interface IRedisService
    {
        IDatabase RedisClient { get; }

        Dictionary<string, string> HashGetAllJsonValue(string key, CommandFlags flags = CommandFlags.None);
        T HashGetJsonValue<T>(string key, string childKey, CommandFlags flags = CommandFlags.None);
        void HashSetJsonValue<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None);
        void SetRedisConfig(string connString);
    }
}