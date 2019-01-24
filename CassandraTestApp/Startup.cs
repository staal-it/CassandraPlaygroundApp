using Microsoft.Extensions.DependencyInjection;

namespace CassandraTestApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICounter, Counter>();
            services.AddTransient<IDbFiller, DbFiller>();
            services.AddTransient<IDbReader, DbReader>();
            services.AddTransient<IFileCreator, FileCreator>();
            services.AddSingleton<ITimedCounterReader, TimedCounterReader>();
        }
    }
}