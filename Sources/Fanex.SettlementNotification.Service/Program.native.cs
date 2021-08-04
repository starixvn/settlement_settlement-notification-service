using SettlementNotification.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;

namespace Fanex.SettlementNotification.Service
{
    public class Program
    {
        private static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
       WebHost.CreateDefaultBuilder(args)
           .UseStartup<Startup>()

           .ConfigureServices((hostContext, services) =>
           {
               services
               .Configure<AppSettingsKey>(hostContext.Configuration)
               .AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettingsKey>>().Value);
               services.AddHostedService<Startup>();
               services.ConfigInternalService(hostContext.Configuration);
           });
    }
}