using Serilog;

namespace Project.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            var statusCode = context.Response.StatusCode;
            if (statusCode < 200 || statusCode >= 300)
            {
                Log.Information("---> request {Path} returns {StatusCode}", context.Request.Path, statusCode);
            }
        }
    }
}
