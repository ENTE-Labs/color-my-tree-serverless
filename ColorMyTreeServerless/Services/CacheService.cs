using System;
using System.Threading.Tasks;
using ColorMyTree.Extensions;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ColorMyTree.Services
{
    public class CacheService
    {
        private readonly IDatabase _redisCache;

        public CacheService(CacheConnectionService cacheConn)
        {
            _redisCache = cacheConn.GetDatabase();
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, TimeSpan? expiry = null)
        {
            return await _redisCache.GetOrCreateAsync(key, factory, expiry ?? TimeSpan.FromDays(1));
        }
        public async Task SetAsync(string key, object data, TimeSpan? expiry = null)
        {
            await _redisCache.StringSetAsync(key, JsonConvert.SerializeObject(data), expiry ?? TimeSpan.FromDays(1));
        }

        public async Task DeleteAsync(string key)
        {
            await _redisCache.KeyDeleteAsync(key);
        }
    }
}
