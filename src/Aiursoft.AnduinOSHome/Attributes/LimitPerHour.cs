using System.Net;
using Aiursoft.CSTools.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace Aiursoft.AnduinOSHome.Attributes;

/// <summary>
/// An attribute that limits the number of requests per hour. Add this attribute to an ASP.NET Core controller or action method.
/// </summary>
/// <param name="limit">The request limit per hour. If set to 3, at most 3 requests are allowed within an hour.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LimitPerHour(int limit = 3) : ActionFilterAttribute
{
    private readonly int _actualLimit = limit + 1;
    public static bool GlobalEnabled = true;
    public static bool KeepFunctioningInUnitTest = false;

    private class AtomicCounter
    {
        private int _count;
        public int Increment() => Interlocked.Increment(ref _count);
    }

    private static string GetIp(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null)
        {
            return "unknown-ip";
        }

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        return remoteIp.ToString();
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        if (!GlobalEnabled)
        {
            return;
        }

        if (EntryExtends.IsInUnitTests() && !KeepFunctioningInUnitTest)
        {
            return;
        }

        var logger = context.HttpContext.RequestServices.GetService<ILogger<LimitPerHour>>();
        var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();

        if (cache == null)
        {
            throw new InvalidOperationException("IMemoryCache service is not registered. Rate limiting is disabled.");
        }

        var path = context.HttpContext.Request.Path.ToString().ToLowerInvariant();
        var ip = GetIp(context.HttpContext);
        var key = $"rate_limit_hour:{ip}:{path}";

        var counter = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return new AtomicCounter();
        });

        var currentCount = counter!.Increment();

        if (currentCount >= _actualLimit)
        {
            logger?.LogWarning("Rate limit exceeded for {Key} (Count: {Count})", key, currentCount);
            context.HttpContext.Response.Headers.Append("Retry-After", "3600");
            context.Result = new ObjectResult(new
            {
                code = 429,
                message = "Too many requests. You can only rate up to 3 times per hour."
            })
            {
                StatusCode = (int)HttpStatusCode.TooManyRequests
            };
            return;
        }

        var remaining = Math.Max(0, limit - currentCount);
        context.HttpContext.Response.Headers.Append("x-rate-limit-limit", "1h");
        context.HttpContext.Response.Headers.Append("x-rate-limit-remaining", remaining.ToString());
    }
}
