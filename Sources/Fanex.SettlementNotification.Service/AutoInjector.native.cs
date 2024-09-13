using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
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
using StackExchange.Redis;
using System;
using System.Collections.Generic;

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
        services.AddSingleton<MatchService, MatchServiceImp>();
        services.AddSingleton<ConfirmRefundService, ConfirmRefundServiceImp>();

        StartUmSetting(configuration);
        StartLogSetting(configuration);
        ConfigureRedis(services, configuration);

        AppConfigService.Config.LetstalkServiceApi = configuration.GetSection("Letstalk:ServiceUri").Value;
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

    private static void ConfigureRedis(IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("AuthSettings:RedisConfig").Value;
        var applicationName = configuration.GetSection("AuthSettings:ChannelPrefix").Value;

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig}"));

        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = applicationName,
        }));
        services.AddSingleton<ICacheService>(new CacheService());
    }
}