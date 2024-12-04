using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

/// <summary>
/// 
/// </summary>
namespace Backend_Server.Infrastructure
{
    [Route("[controller]")]
    public abstract class CachedBaseController(IMemoryCache cache) : ControllerBase
    {
        protected readonly IMemoryCache _cache = cache;

        protected async Task<T> GetCachedAsync<T>(
            string cacheKey, 
            Func<Task<T>> getter,
            TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedValue))
            {
                Log.Information("Cache hit for key: {Key}", cacheKey);

                if (cachedValue == null)
                {
                    Log.Warning("Cache value for key: {Key} is null", cacheKey);
                    throw new InvalidOperationException("Cache miss");
                }

                return cachedValue;
            }

            Log.Information("Cache miss for key: {Key}", cacheKey);
            var value = await getter();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(5))
                .SetSize(1);

            _cache.Set(cacheKey, value, cacheOptions);
            return value;
        }

        protected async Task<T> ExecuteQueryWithRetryAsync<T>(
            Func<Task<T>> query,
            int maxRetries = 3)
        {
            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    return await query();
                }
                catch (Exception ex) when (i < maxRetries && 
                    (ex is DbUpdateConcurrencyException || 
                    ex is DbUpdateException ||
                    ex is TimeoutException))
                {
                    Log.Warning(ex, "Query attempt {Attempt} failed, retrying...", i + 1);
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, i) * 100));
                }
            }
            throw new Exception($"Query failed after {maxRetries} attempts");
        }
    }
}