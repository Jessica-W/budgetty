using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Budgetty.Mvc.Filters
{
    public class LogCsrfFailureFilter : IAsyncAlwaysRunResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate? next)
        {
            if (context.Result is AntiforgeryValidationFailedResult)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<LogCsrfFailureFilter>>();

                logger?.Log(LogLevel.Warning, "Missing or invalid CSRF token");
            }

            return next?.Invoke() ?? Task.CompletedTask;
        }
    }
}
