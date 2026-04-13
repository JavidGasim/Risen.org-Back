using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Risen.Business.Exceptions;
using System.Text.Json;

namespace Risen.Web.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (BadRequestException ex)
            {
                await WriteResponse(ctx, 400, ex.Message);
            }
            catch (NotFoundException ex)
            {
                await WriteResponse(ctx, 404, ex.Message);
            }
            catch (ForbiddenException ex)
            {
                await WriteResponse(ctx, 403, ex.Message);
            }
            catch (ValidationException ex)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "application/json";

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                await ctx.Response.WriteAsJsonAsync(new { errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error: {Message}", ex.Message);
                await WriteResponse(ctx, 500, "Xəta baş verdi. Zəhmət olmasa yenidən cəhd edin.");
            }
        }

        private static async Task WriteResponse(HttpContext ctx, int statusCode, string message)
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new { message });
            await ctx.Response.WriteAsync(body);
        }
    }
}
