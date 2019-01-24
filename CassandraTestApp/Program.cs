using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CassandraTestApp
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            var startup = new Startup();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            Console.WriteLine("Enter R for read, F for fill...");
            var input = Console.ReadLine();

            IDbService service = null;

            if (input.ToLower() == "r")
            {
                service = serviceProvider.GetService<IDbReader>();
                await StartTimer(serviceProvider);
            }
            else if (input.ToLower() == "f")
            {
                service = serviceProvider.GetService<IDbFiller>();
                await StartTimer(serviceProvider);
            }

            await service.Start();

            Console.WriteLine("Press enter key to exit");
        }

        private static async Task StartTimer(IServiceProvider serviceProvider)
        {
            var timer = serviceProvider.GetService<ITimedCounterReader>();

            await Task.Run(() => { timer.ExecuteAsync(); });
        }
    }
}