using Fanex.Caching;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.LetstalkService.Core.Services;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Sentry;
using Fanex.RemotingRef;
using Microsoft.Extensions.Configuration;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Transport;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.SettlementNotification.Service
{
    public partial class Startup
    {
        private void StartUMSetting()
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
               .ConnectTo(new Uri(configuration.GetSection("Fanex.RemotingRef.Service.Url").Value))
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
}