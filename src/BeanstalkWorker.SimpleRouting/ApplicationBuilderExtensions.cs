using Microsoft.AspNetCore.Builder;

namespace BeanstalkWorker.SimpleRouting
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHeaderRouting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HeaderRoutingMiddleware>();
        }
    }
}
