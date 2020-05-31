using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BeanstalkWorker.SimpleRouting.SampleWorker
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            // The simple routing middleware needs to be added **before** configuring endpoint routing
            app.UseHeaderRouting();

            app.UseRouting();

            app.UseEndpoints(endpoint => endpoint.MapControllers());
        }
    }
}
