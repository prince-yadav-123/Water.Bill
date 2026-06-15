using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Water.Bill.Application.Interfaces;

namespace Water.Bill.Infrastructure.Services;

public class OtpThrottleService : IOtpThrottleService
{
    private const int MaxRequestsPerSubject = 4;
    private const int MaxRequestsPerIp = 12;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OtpThrottleService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool TryConsumeRequest(string purpose, string? subjectKey, out TimeSpan retryAfter)
    {
        retryAfter = TimeSpan.Zero;

        var ipKey = BuildKey("ip", purpose, GetClientIp());
        var subjectCacheKey = string.IsNullOrWhiteSpace(subjectKey)
            ? null
            : BuildKey("subject", purpose, subjectKey);

        var now = DateTimeOffset.UtcNow;
        var ipCounter = GetOrCreateCounter(ipKey, now);
        var subjectCounter = subjectCacheKey is null ? null : GetOrCreateCounter(subjectCacheKey, now);

        if (IsLimitReached(ipCounter, MaxRequestsPerIp, now, out retryAfter))
            return false;

        if (subjectCounter is not null && IsLimitReached(subjectCounter, MaxRequestsPerSubject, now, out retryAfter))
            return false;

        ipCounter.Count++;
        subjectCounter?.Increment();
        return true;
    }

    private OtpThrottleCounter GetOrCreateCounter(string cacheKey, DateTimeOffset now)
    {
        if (_cache.TryGetValue(cacheKey, out OtpThrottleCounter? existing) && existing is not null)
        {
            if (now - existing.WindowStart >= Window)
            {
                existing.Reset(now);
            }

            return existing;
        }

        var counter = new OtpThrottleCounter(now);
        _cache.Set(cacheKey, counter, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Window
        });
        return counter;
    }

    private static bool IsLimitReached(OtpThrottleCounter counter, int maxCount, DateTimeOffset now, out TimeSpan retryAfter)
    {
        if (counter.Count < maxCount)
        {
            retryAfter = TimeSpan.Zero;
            return false;
        }

        var remaining = Window - (now - counter.WindowStart);
        retryAfter = remaining > TimeSpan.Zero ? remaining : TimeSpan.FromMinutes(1);
        return true;
    }

    private string GetClientIp()
        => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()?.Trim() ?? "unknown";

    private static string BuildKey(string scope, string purpose, string value)
        => $"otp-throttle:{scope}:{purpose.Trim().ToLowerInvariant()}:{value.Trim().ToLowerInvariant()}";

    private sealed class OtpThrottleCounter
    {
        public OtpThrottleCounter(DateTimeOffset windowStart)
        {
            WindowStart = windowStart;
        }

        public DateTimeOffset WindowStart { get; private set; }

        public int Count { get; set; }

        public void Increment() => Count++;

        public void Reset(DateTimeOffset now)
        {
            WindowStart = now;
            Count = 0;
        }
    }
}
