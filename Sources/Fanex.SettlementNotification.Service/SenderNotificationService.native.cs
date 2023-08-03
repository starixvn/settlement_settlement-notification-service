using Fanex.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SettlementNotification.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fanex.SettlementNotification.Service;

public class SenderNotificationService : IHostedService
{
    private readonly AutoNotificationController autoNotificationController;

    private readonly IMaintenanceService maintenanceService;

    private readonly IConfiguration configuration;

    private bool running;

    private int counter;

    public SenderNotificationService(IConfiguration configuration, IMaintenanceService maintenanceService, AutoNotificationController autoNotificationController)
    {
        this.configuration = configuration;
        this.maintenanceService = maintenanceService;
        this.autoNotificationController = autoNotificationController;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        running = true;
        var thread = new Thread(ThreadProcess);
        Console.WriteLine($"Starting at {DateTime.Now}");
        thread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        running = false;
        Console.WriteLine($"Stopping at {DateTime.Now}");
        return Task.CompletedTask;
    }

    private void ThreadProcess()
    {
        //while (running)
        //{
        //    if (maintenanceService.IsMaintenance) continue;

        MainProcess();
        var interval = configuration.GetSection("ServiceInterval").Value ?? "0";
        Thread.Sleep(int.Parse(interval));
        //}
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
        var collectionInterval = configuration.GetSection("GarbageCollectorInterval").Value ?? "0";
        if (counter >= int.Parse(collectionInterval))
        {
            counter = 0;
            GC.WaitForPendingFinalizers();
        }
        var interval = configuration.GetSection("ServiceInterval").Value ?? "0";

        counter += int.Parse(interval);
    }
}