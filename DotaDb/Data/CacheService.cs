using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class CacheService
    {
        private readonly Dictionary<string, SemaphoreSlim> lockSet = new Dictionary<string, SemaphoreSlim>();
        private readonly SemaphoreSlim s = new SemaphoreSlim(1, 1);
        private readonly IMemoryCache cache;

        public CacheService(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> populator, TimeSpan expiration)
        {
            // Check if already in cache, doesn't require a lock
            if (!cache.TryGetValue(key, out T value))
            {
                // Cache miss, try to get a lock on this critical path
                await s.WaitAsync();

                try
                {
                    // Try again now that we're locked
                    if (!cache.TryGetValue(key, out value))
                    {
                        // Another cache miss, do long-running population
                        value = await populator();

                        // Set the results of the population for other consumers
                        cache.Set(key, value, new MemoryCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = expiration,
                        });
                    }
                }
                finally
                {
                    s.Release();
                }
            }

            return value;
        }
    }
}