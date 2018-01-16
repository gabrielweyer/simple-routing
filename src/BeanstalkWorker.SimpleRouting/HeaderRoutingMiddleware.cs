using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace BeanstalkWorker.SimpleRouting
{
    public class HeaderRoutingMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string TaskHeaderName = $"X-Aws-Sqsd-Attr-{RoutingConstants.HeaderName}";

        public HeaderRoutingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context, ILogger<HeaderRoutingMiddleware> logger)
        {
            if (!"POST".Equals(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Task.CompletedTask;
            }

            if (!"/".Equals(context.Request.Path))
            {
                logger.LogWarning("Request path does not match '/', instead it is {WorkerRequestPath}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Task.CompletedTask;
            }

            StringValues task = context.Request.Headers[TaskHeaderName];

            if (task.Count != 1)
            {
                logger.LogWarning("Request Task header should contain one task {WorkerTasks}", task);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return Task.CompletedTask;
            }

            context.Request.Path = $"/{task.Single()}";

            return _next(context);
        }
    }
}