using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BeanstalkWorker.SimpleRouting.SampleWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Build().Run();
        }

        private static IHostBuilder BuildWebHost(string[] args)
        {
            return Host.
                CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
