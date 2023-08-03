using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Core.Services;
using Fanex.Logging;
using Fanex.Logging.Sentry.RavenClient;
using Fanex.RemotingRef;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;
using SettlementNotification.Services;
using SharpRaven;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.SettlementNotification.Service;

public static class AutoInjector
{
    public static void ConfigInternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<RefDataService, RefDataServiceImp>();
        services.AddTransient<IRemotingRefService, RemotingRefService>();
        services.AddTransient<IMaintenanceService, MaintenanceServiceImp>();
        services.AddSingleton<AutoNotificationController, AutoNotificationControllerImp>();
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<ContentService, ContentServiceImp>();
        services.AddSingleton<NotificationService, NotificationServiceImp>();
        services.AddSingleton<NotificationPoolService, NotificationPoolServiceImp>();
        services.AddSingleton<QueryService, QueryServiceImp>();
        services.AddSingleton(new UMClientConfiguration().Initialize(configuration));
        services.AddSingleton<IUMCacheService, UMCacheService>();
        services.AddSingleton<IUMRestClient, UMRestClient>();
        services.AddSingleton<ONEbook.UM.Services.IMaintenanceService, ONEbook.UM.Services.MaintenanceService>();
        services.AddSingleton<ICacheService>(new CacheService());
        services.AddSingleton<LetstalkClientService, LetstalkClientServiceImp>();
        services.AddSingleton<SendMessageService, SendMessageServiceImp>();
        StartUmSetting(configuration);
        StartDataBaseSetting(configuration);
        StartLogSetting(configuration);
        ConfigureRedis(services, configuration);

        AppConfigService.Config.LetstalkServiceApi = configuration.GetSection("LetstalkServiceApi").Value;
        RefDataTypeContainer
            .ConnectTo(new Uri(configuration.GetSection("Fanex.RemotingRef.Service.Url").Value ?? string.Empty))
            .FromApplication("Settlement");
    }

    private static void StartUmSetting(IConfiguration configuration)
    {
        var umConfig = new UMClientConfiguration().Initialize(configuration);

        UMClientManager
            .UseConfig(umConfig)
            .UseCacheService(new UMCacheService(new CacheService()))
            .UseRestClient(new UMRestClient(umConfig));
    }

    private static void StartDataBaseSetting(IConfiguration configuration)
    {
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
    }

    private static void StartLogSetting(IConfiguration configuration)
    {
        var logEngine = new List<ILogEngine>();

        var sentryEngineOptions = new SentryEngineOptions
        {
            Dsn = new Dsn(configuration.GetSection("SentryToken").Value)
        };

        logEngine.Add(new SentryLogging(sentryEngineOptions));

        LogManager.SetDefaultLogCategory(configuration.GetSection("LogCategory").Value).Use(logEngine);
    }

    private static void ConfigureRedis(IServiceCollection service, IConfiguration configuration)
    {
        var redisHost = configuration.GetSection("Redis.Server").Value;
        var redisPort = configuration.GetSection("Redis.Port").Value;
        var redisPassword = configuration.GetSection("Redis.Password").Value;
        var redisDatabase = configuration.GetSection("Redis.DefaultDatabase").Value;
        service.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = $"{redisHost}:{redisPort},password={redisPassword},defaultDatabase={redisDatabase}",
            ApplicationName = configuration.GetSection("Redis.ApplicationName").Value,
        }));
    }
}