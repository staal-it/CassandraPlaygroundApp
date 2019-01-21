using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace DatabaseFiller
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            // Startup.cs finally :)
            var startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var service = serviceProvider.GetService<IDbFiller>();

            var timer = serviceProvider.GetService<ITimedHostedService>();

            await Task.Run(() =>{ timer.ExecuteAsync(); });

            await service.Start();
        }
    }
}