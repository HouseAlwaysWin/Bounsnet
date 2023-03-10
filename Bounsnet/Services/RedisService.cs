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

        public async Task AddHashJsonValue<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None)
        {
            var newData = data.Select(d => new HashEntry(d.Key, JsonConvert.SerializeObject(d.Value))).ToArray();
            await RedisClient.HashSetAsync(key, newData, flags);
        }

        public Dictionary<string, string> GetHashAllJsonValue(string key, CommandFlags flags = CommandFlags.None)
        {
            var allData = RedisClient.HashGetAll(key, flags);
            var dict = allData.ToDictionary(k => k.Name.ToString(), v => v.Value.ToString());
            return dict;
        }

        public T GetHashJsonValue<T>(string key, string childKey, CommandFlags flags = CommandFlags.None)
        {
            var allData = GetHashAllJsonValue(key, flags);
            if (allData.ContainsKey(childKey))
            {
                var data = JsonConvert.DeserializeObject<T>(allData[childKey]);
                return data;
            }
            return default(T);
        }

        /// <summary>
        /// Add All data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task AddOrUpdateAllHashJsonValueAsync<T>(string key, Dictionary<string, T> data, CommandFlags flags = CommandFlags.None)
        {
            var newData = data.Select(d => new HashEntry(d.Key, JsonConvert.SerializeObject(d.Value))).ToArray();
            await RedisClient.HashSetAsync(key, newData, flags);
        }

        public async Task AddOrUpdateHashJsonValueAsync<T>(string key, string childKey, T data, CommandFlags flags = CommandFlags.None)
        {
            var allData = await GetAllHashJsonValueAsync<T>(key);
            if (!allData.ContainsKey(childKey))
            {
                allData.Add(childKey, data);
            }
            else
            {
                allData[childKey] = data;
            }

            await AddOrUpdateAllHashJsonValueAsync(key, allData, flags);
        }

        public async Task<Dictionary<string, T>> GetAllHashJsonValueAsync<T>(string key, CommandFlags flags = CommandFlags.None)
        {
            var allData = await RedisClient.HashGetAllAsync(key, flags);
            var dict = allData.ToDictionary(k => k.Name.ToString(), v => JsonConvert.DeserializeObject<T>(v.Value));
            return dict;
        }

        public async Task<T> GetHashJsonValueAsync<T>(string key, string childKey, CommandFlags flags = CommandFlags.None)
        {
            var allData = await GetAllHashJsonValueAsync<T>(key, flags);
            if (allData.ContainsKey(childKey))
            {
                return allData[childKey];
            }
            return default(T);
        }

    }
}
