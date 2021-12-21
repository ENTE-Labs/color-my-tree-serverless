using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace ColorMyTree.Extensions
{
    public static class CacheDatabaseExtensions
    {
        public static async Task<T> GetOrCreateAsync<T>(this IDatabase database, RedisKey key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            try
            {
                T returnValue;

                var cachedValue = database.StringGet(key);
                if (!cachedValue.HasValue)
                {
                    returnValue = await factory();
                    if (returnValue != null)
                    {
                        await database.StringSetAsync(key, JsonConvert.SerializeObject(returnValue), expiry ?? TimeSpan.FromDays(1));
                    }
                }
                else
                {
                    returnValue = JsonConvert.DeserializeObject<T>(cachedValue);
                }

                return returnValue;
            }
            catch
            {
                return await factory();
            }
        }
    }
}
