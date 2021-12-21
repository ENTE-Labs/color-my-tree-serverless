using System;
using System.Threading.Tasks;
using ColorMyTree.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ColorMyTree.Services
{
    public class CacheService
    {
        private readonly IDatabase _redisCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(CacheConnectionService cacheConn, ILogger<CacheService> logger)
        {
            _logger = logger;
            _redisCache = cacheConn.GetDatabase();
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, TimeSpan? expiry = null)
        {
            _logger.LogInformation($"Cache: start get or create {key}");

            var value = await _redisCache.GetOrCreateAsync(key, factory, expiry ?? TimeSpan.FromDays(1));

            _logger.LogInformation($"Cache: start get or create {key}");

            return value;
        }
        public async Task SetAsync(string key, object data, TimeSpan? expiry = null)
        {
            _logger.LogInformation($"Cache: start set {key}");

            await _redisCache.StringSetAsync(key, JsonConvert.SerializeObject(data), expiry ?? TimeSpan.FromDays(1));

            _logger.LogInformation($"Cache: finish set {key}");
        }

        public async Task DeleteAsync(string key)
        {
            _logger.LogInformation($"Cache: start del {key}");

            await _redisCache.KeyDeleteAsync(key);

            _logger.LogInformation($"Cache: finish del {key}");
        }
    }
}
