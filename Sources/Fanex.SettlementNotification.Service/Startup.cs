using SettlementNotification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Logging;

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
            var autoRefundThread = new Thread(ThreadProcess);
            Console.WriteLine($"Auto Payout Other Sports Starting at { DateTime.Now }");
            autoRefundThread.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            running = false;
            Console.WriteLine($"Auto Payout Other Sports Stopping at { DateTime.Now }");
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
    }
}