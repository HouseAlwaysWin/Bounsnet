using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Bounsnet.Services
{
    public class RedisService : IRedisService
    {
        private static ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> _connectionPools = new();

        private string _connString;
        private Lazy<ConnectionMultiplexer> _redisConn;
        private ConnectionMultiplexer Instance => _redisConn.Value;
        public IDatabase RedisClient => this.Instance.GetDatabase();

        public RedisService()
        {
            SetRedisConfig("localhost");
        }

        public RedisService(string connString)
        {
            SetRedisConfig(connString);
        }

        public void SetRedisConfig(string connString)
        {
            _connString = connString;
            _redisConn = new Lazy<ConnectionMultiplexer>(() =>
            {
                var conn = _connectionPools.GetOrAdd(connString, new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connString)));
                return conn.Value;
            });
        }

        public void HashSetJsonValue<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None)
        {
            var newData = data.Select(d => new HashEntry(d.Key, JsonConvert.SerializeObject(d.Value))).ToArray();
            RedisClient.HashSet(key, newData, flags);
        }

        public Dictionary<string, string> HashGetAllJsonValue(string key, CommandFlags flags = CommandFlags.None)
        {
            var allData = RedisClient.HashGetAll(key, flags);
            var dict = allData.ToDictionary(k => k.Name.ToString(), v => v.Value.ToString());
            return dict;
        }

        public T HashGetJsonValue<T>(string key, string childKey, CommandFlags flags = CommandFlags.None)
        {
            var allData = HashGetAllJsonValue(key, flags);
            if (allData.ContainsKey(childKey))
            {
                var data = JsonConvert.DeserializeObject<T>(allData[childKey]);
                return data;
            }
            return default(T);
        }


    }
}
