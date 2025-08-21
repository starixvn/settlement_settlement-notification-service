using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Fanex.LetstalkService.Client;
using Fanex.Logging;
using Fanex.RemotingRef;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SettlementNotification.Services;
using StackExchange.Redis;

namespace Fanex.SettlementNotification.Service;

public static class AutoInjector
{
    public static void ConfigInternalService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ILogger, Logger>();
        services.AddTransient<RefDataService, RefDataServiceImp>();
        services.AddTransient<IRemotingRefService, RemotingRefService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<AutoNotificationController, AutoNotificationControllerImp>();
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<ContentService, ContentServiceImp>();
        services.AddSingleton<NotificationService, NotificationServiceImp>();
        services.AddSingleton<NotificationPoolService, NotificationPoolServiceImp>();
        services.AddSingleton<QueryService, QueryServiceImp>();
        services.AddSingleton<LetstalkClientService, LetstalkClientServiceImp>();
        services.AddSingleton<SendMessageService, SendMessageServiceImp>();
        services.AddSingleton<MatchService, MatchServiceImp>();
        services.AddSingleton<ConfirmRefundService, ConfirmRefundServiceImp>();
        services.AddTransient<ISettleLogService, SettleLogService>();

        AppConfigService.Config.LetstalkServiceApi = configuration.GetSection("Letstalk:ServiceUri").Value;
        RefDataTypeContainer
            .ConnectTo(new Uri(configuration.GetSection("Fanex.RemotingRef.Service.Url").Value ?? string.Empty))
            .FromApplication("Settlement");
    }

    public static void AddConfigRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Fanex.Caching.Redis.Ex:RedisConfig").Value;
        var channelPrefix = configuration.GetSection("Fanex.Caching.Redis.Ex:ChannelPrefix").Value;
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect($"{redisConfig}"));
        services.AddSingleton<ICache>(new RedisCache(new RedisCacheOptions
        {
            ConnectionString = redisConfig,
            ApplicationName = channelPrefix,
        }));
        services.AddRedisCache(configuration.GetSection("Fanex.Caching.Redis"));
    }
}