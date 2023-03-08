using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Bounsweet.Services
{
    public class RedisService
    {
        private static ConcurrentDictionary<string, Lazy<ConnectionMultiplexer>> _connectionPools = new();

        private string _connString;
        private Lazy<ConnectionMultiplexer> _redisConn;
        private ConnectionMultiplexer Instance => _redisConn.Value;
        public IDatabase RedisClient => this.Instance.GetDatabase();

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

        public void Set<T>(string key, Dictionary<string, T> data, TimeSpan? expiry = default)
        {
            var newData = data.Select(d => new HashEntry(d.Key, d.Value?.ToString())).ToArray();
            RedisClient.HashSet(key, newData);
        }
    }
}
