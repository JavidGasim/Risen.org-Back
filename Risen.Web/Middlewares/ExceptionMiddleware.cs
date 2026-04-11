using Microsoft.Extensions.Caching.Memory;

namespace Risen.Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public ExceptionMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }
        public async Task InvokeAsync(HttpContext ctx)
        {
            try { await _next(ctx); }
            catch (InvalidOperationException ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                ctx.Response.StatusCode = 404;
                await ctx.Response.WriteAsJsonAsync(new { message = ex.Message });
            }
        }
    }
}
