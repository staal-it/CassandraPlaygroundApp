using Microsoft.Extensions.DependencyInjection;

namespace DatabaseFiller
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICounter, Counter>();
            services.AddTransient<IDbFiller, DbFiller>();
            services.AddTransient<ITimedHostedService, TimedHostedService>();
        }
    }
}