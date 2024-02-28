using ecommerce.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ecommerce.Services
{
    public class Redis : IRedis
    {
        private readonly IDistributedCache _cache;

        public Redis(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetCachedDataAsync<T>(string cacheKey)
        {
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (cachedData != null)
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }

            return default;
        }

        public async Task SetCachedDataAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options)
        {
            var serializedData = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(cacheKey, serializedData, options);
        }

        public async Task RemoveCachedDataAsync(string cacheKey)
        {
            await _cache.RemoveAsync(cacheKey);
        }
    }
}
