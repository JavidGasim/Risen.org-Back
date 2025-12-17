using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Risen.Entities.Entities;
using System.Security.Claims;

namespace Risen.Web.Middlewares
{
    public class LastOnlineMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public LastOnlineMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<CustomIdentityUser> userManager)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var cacheKey = $"lastonline:{userId}";

                    // hər request-də DB yazmamaq üçün 5 dəqiqə throttle
                    if (!_cache.TryGetValue(cacheKey, out _))
                    {
                        var user = await userManager.FindByIdAsync(userId.ToString());
                        if (user is not null)
                        {
                            user.LastOnlineAtUtc = DateTime.UtcNow;
                            await userManager.UpdateAsync(user);
                        }

                        _cache.Set(cacheKey, true, TimeSpan.FromMinutes(5));
                    }
                }
            }

            await _next(context);
        }
    }
}
