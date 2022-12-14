using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Budgetty.Mvc
{
    public class LogCsrfFailureFilter : IAsyncAlwaysRunResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is AntiforgeryValidationFailedResult reult)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<LogCsrfFailureFilter>>();

                logger?.Log(LogLevel.Warning, "Missing or invalid CSRF token");
            }

            return next();
        }
    }
}
