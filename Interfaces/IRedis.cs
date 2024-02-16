using Microsoft.Extensions.Caching.Distributed;

namespace ecommerce.Interfaces
{
    public interface IRedis
    {
        public Task<T?> GetCachedDataAsync<T>(string cacheKey);
        public Task SetCachedDataAsync<T>(string cacheKey, T data, DistributedCacheEntryOptions options);
        public Task RemoveCachedDataAsync(string cacheKey);
    }
}
