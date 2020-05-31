using Amazon;
using Amazon.SQS;
using BeanstalkWorker.SimpleRouting.Core.Options;
using Microsoft.AspNetCore.Builder;
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

            services
                .AddOptions()
                .AddControllers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoint => endpoint.MapControllers());
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

                var accessKeyId = _configuration.GetValue<string>("Aws:Queue:AccessKeyId");
                var secretAccessKey = _configuration.GetValue<string>("Aws:Queue:SecretAccessKey");

                return new AmazonSQSClient(accessKeyId, secretAccessKey, regionEndpoint);
            });
            services.AddScoped<ISqsClient, SqsClient>();
        }
    }
}
