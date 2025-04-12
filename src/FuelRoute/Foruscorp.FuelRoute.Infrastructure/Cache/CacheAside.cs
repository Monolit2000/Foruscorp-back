using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Foruscorp.FuelRoutes.Infrastructure.Cache
{
    public static class CacheAside
    {
        private static readonly MemoryCacheEntryOptions Default = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };

        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(30);

        public static async Task<T?> GetOrCreateAsync<T>(
            this IMemoryCache memoryCache,
            string key,
            Func<CancellationToken, Task<T>> factory,
            TimeSpan? expiration = null,
            CancellationToken cancellationToken = default)
        {
            //memoryCache.TryGetValue(key, out T value);
            //if (value is not null)
            //{
            //    return value;
            //}

            //var result = await factory(cancellationToken);

            //if (result is null)
            //    return default;

            //memoryCache.Set(key, result, expiration ?? DefaultExpiration);


           var value = await memoryCache.GetOrCreateAsync(key, async entry =>
           {
               entry.SetOptions(Default);
               entry.AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration;
               return await factory(cancellationToken);
           });


            return value;
        }
    }
}
