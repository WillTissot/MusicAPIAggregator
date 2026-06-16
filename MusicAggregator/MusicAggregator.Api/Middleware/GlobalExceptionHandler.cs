using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MusicAggregator.Api.Middleware
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
        {
            var (status, title) = exception switch
            {
                ArgumentException or BadHttpRequestException
                    => (StatusCodes.Status400BadRequest, "Invalid request."),
                UnauthorizedAccessException
                    => (StatusCodes.Status401Unauthorized, "Unauthorized."),
                KeyNotFoundException
                    => (StatusCodes.Status404NotFound, "Resource not found."),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception for {Path}", httpContext.Request.Path);
            else
                _logger.LogWarning(exception, "{Status} for {Path}", status, httpContext.Request.Path);

            var problem = new ProblemDetails { Status = status, Title = title };
            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, ct);
            return true;   // handled
        }
    }
}
