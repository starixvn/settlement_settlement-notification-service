using SettlementNotification.Services;
using Fanex.Caching;
using Fanex.Data.Repository;
using Fanex.LetstalkService.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Services;
using ONEbook.UM.Transport;
using Fanex.RemotingRef;

namespace Fanex.SettlementNotification.Service
{
    public static class AutoInjector
    {
        public static void ConfigInternalService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<RefDataService, RefDataServiceImp>();
            services.AddTransient<IRemotingRefService, RemotingRefService>();
            services.AddTransient<MaintainanceService, MaintainanceServiceImp>();
            services.AddSingleton<AutoNotificationController, AutoNotificationControllerImp>();
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<ContentService, ContentServiceImp>();
            services.AddSingleton<NotificationService, NotificationServiceImp>();
            services.AddSingleton<NotificationPoolService, NotificationPoolServiceImp>();
            services.AddSingleton<QueryService, QueryServiceImp>();
            services.AddSingleton(new UMClientConfiguration().Initialize(configuration));
            services.AddSingleton<IUMCacheService, UMCacheService>();
            services.AddSingleton<IUMRestClient, UMRestClient>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<ICacheService>(new CacheService());
            services.AddSingleton<LetstalkClientService, LetstalkClientServiceImp>();
            services.AddSingleton<SendMessageService, SendMessageServiceImp>();
        }
    }
}