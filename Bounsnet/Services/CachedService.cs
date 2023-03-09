using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bounsnet.Services
{
    public class CachedService : ICachedService
    {
        private readonly IDistributedCache _db;

        public CachedService(
            IDistributedCache db
            )
        {
            this._db = db;
        }

        /// <summary>
        /// Get Cached Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetJsonDataAsync<T>(string key) where T : class
        {
            string data = await _db.GetStringAsync(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return null;
        }

        /// <summary>
        ///  Get Cached Sync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetJsonData<T>(string key) where T : class
        {
            string data = _db.GetString(key);

            if (!string.IsNullOrEmpty(data))
            {
                return JsonConvert.DeserializeObject<T>(data);
            }
            return null;
        }

        public async Task<bool> SetJsonDataAsync<T>(string key, T data, TimeSpan? time = null) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            await _db.SetStringAsync(
                  key, JsonConvert.SerializeObject(data),
                  options);

            return true;
        }

        public bool SetJsonData<T>(string key, T data, TimeSpan? time) where T : class
        {
            if (!time.HasValue)
            {
                time = TimeSpan.FromDays(30);
            }

            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = time
            };

            _db.SetString(key, JsonConvert.SerializeObject(data),
                  options);

            return true;
        }

        public async Task<T> GetAndSetJsonDataAsync<T>(string key, T data, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetJsonDataAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var result = await SetJsonDataAsync<T>(key, data, time);
            if (result)
            {
                return await GetJsonDataAsync<T>(key);
            }
            return default(T);
        }

        public async Task<T> GetAndSetAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = await GetJsonDataAsync<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = await acquire();

            if (data != null)
            {
                await SetJsonDataAsync<T>(key, data, time);
            }

            return data;
        }

        public T GetAndSet<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class
        {
            var cachedData = GetJsonData<T>(key);
            if (cachedData != null)
            {
                return cachedData;
            }
            var data = acquire();

            if (data != null)
            {
                SetJsonData<T>(key, data, time);
            }
            return data;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            await _db.RemoveAsync(id);
            return true;
        }
    }
}
