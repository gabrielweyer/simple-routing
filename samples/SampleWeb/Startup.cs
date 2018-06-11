using Amazon;
using Amazon.SQS;
using BeanstalkWorker.SimpleRouting.Core.Logic;
using BeanstalkWorker.SimpleRouting.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeanstalkWorker.SimpleRouting.SampleWeb
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddServices(services);

            ConfigureOptions(services);

            services.AddOptions();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new {controller = "Send", action = "Nothing"});
            });
        }

        private void ConfigureOptions(IServiceCollection services)
        {
            services.Configure<QueueOptions>(_configuration.GetSection("Aws:Queue"));
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSQS>(sp =>
            {
                var systemName = _configuration.GetValue<string>("Aws:RegionSystemName");
                var regionEndpoint = RegionEndpoint.GetBySystemName(systemName);

                return new AmazonSQSClient(regionEndpoint);
            });
            services.AddScoped<ISqsClient, SqsClient>();
        }
    }
}
