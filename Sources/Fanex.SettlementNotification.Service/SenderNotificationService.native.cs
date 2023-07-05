using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.LetstalkService.Core.Services;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.RemotingRef;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using SettlementNotification.Services;
using SharpRaven;

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

        StartUmSetting();
        ConfigureFanex();
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
        while (running)
        {
            if (maintenanceService.IsMaintenance) continue;

            MainProcess();
            var interval = configuration.GetSection("ServiceInterval").Value ?? "0";
            Thread.Sleep(int.Parse(interval));
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
        var collectionInterval = configuration.GetSection("GarbageCollectorInterval").Value ?? "0";
        if (counter >= int.Parse(collectionInterval))
        {
            counter = 0;
            GC.WaitForPendingFinalizers();
        }
        var interval = configuration.GetSection("ServiceInterval").Value ?? "0";

        counter += int.Parse(interval);
    }

    private void StartUmSetting()
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }

    private void ConfigureFanex()
    {
        AppConfigService.Config.LetstalkServiceApi = configuration.GetSection("LetstalkServiceApi").Value;

        RefDataTypeContainer
            .ConnectTo(new Uri(configuration.GetSection("Fanex.RemotingRef.Service.Url").Value ?? string.Empty))
            .FromApplication("Settlement");

        var connections = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .AsEnumerable()
            .ToDictionary(
                x => x.GetValue<string>("Name"),
                y => new ConnectionConfiguration(y.GetValue<string>("Name"), y.GetValue<string>("ConnectionString"), y.GetValue<string>("ProviderName"))
            );

        DbSettingProviderManager
            .StartNewSession()
            .Use(connections)
            .WithSqlServer()
            .UseDefaultDbSettingProvider(resourcePath: configuration.GetSection("StoredProcedurePath").Value)
            .Run();

        LogManager
            .SetDefaultLogCategory(configuration.GetSection("LogCategory").Value)
            .Use(new List<ILogEngine>
            {
                new SentryLogging(new SentryEngineOptions
                {
                    Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
                })
            });

        ExceptionHandler
            .UseConfig(new LoggingConfiguration())
            .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo);
    }
}