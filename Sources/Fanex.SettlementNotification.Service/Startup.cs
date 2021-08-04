using SettlementNotification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.SettlementNotification.Service
{
    public partial class Startup : IHostedService
    {
        private readonly AutoNotificationController autoNotificationController;

        private readonly MaintainanceService maintainanceService;

        public readonly IConfiguration configuration;

        private bool running;

        private int counter;

        public Startup(IConfiguration configuration, MaintainanceService maintainanceService, AutoNotificationController autoNotificationController)
        {
            this.configuration = configuration;
            this.maintainanceService = maintainanceService;
            this.autoNotificationController = autoNotificationController;

            StartUMSetting();
            ConfigureFanex();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            running = true;
            var thread = new Thread(ThreadProcess);
            Console.WriteLine($"Starting at { DateTime.Now }");
            thread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
            Console.WriteLine($"Stopping at { DateTime.Now }");
            return Task.CompletedTask;
        }

        private void ThreadProcess()
        {
            while (running)
            {
                if (!maintainanceService.IsMaintainance)
                {
                    MainProcess();
                    Thread.Sleep(int.Parse(configuration.GetSection("ServiceInterval").Value));
                }
            }
        }

        private void MainProcess()
        {
            try
            {
                autoNotificationController.Process();
                HandleGarbageCollector();
            }
            catch (Exception exception)
            {
                LogService.Error("Exception in main thread - Process:", exception);
            }
        }

        private void HandleGarbageCollector()
        {
            if (counter >= int.Parse(configuration.GetSection("GarbageCollectorInterval").Value))
            {
                counter = 0;
                GC.WaitForPendingFinalizers();
            }

            counter += int.Parse(configuration.GetSection("ServiceInterval").Value);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddControllers();
            services.AddRazorPages();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}