using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Net;
using System.Text;
using Budgetty.Domain;
using Budgetty.Domain.Exceptions;
using JetBrains.Annotations;

namespace Budgetty.Mvc.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        [UsedImplicitly]
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (Activity.Current != null)
                {
                    context.Response.Headers.TryAdd("TraceId", Activity.Current.RootId);
                }

                await _next(context);
            }
            catch (SecurityViolationException ex)
            {
                _logger.LogWarning(LogEventId.AccessDenied, ex, ex.Message);

                await SetResponse(context, HttpStatusCode.Forbidden, "Access Denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEventId.UnhandledException, ex, ex.Message);

                await SetResponse(context, HttpStatusCode.InternalServerError, "An unexpected error has occurred");
            }
        }

        private static async Task SetResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.StatusCode = (int)statusCode;

            await using var sw = new HttpResponseStreamWriter(context.Response.Body, Encoding.UTF8);
            await sw.WriteLineAsync(message);
        }
    }
}
