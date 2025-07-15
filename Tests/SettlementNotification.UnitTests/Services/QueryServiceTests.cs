using Fanex.Caching;
using Fanex.Caching.Redis;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SettlementNotification.UnitTests.Services;

public class QueryServiceTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();

    private readonly ICache cache = new RedisCache(new RedisCacheOptions
    {
        ConnectionString = "10.18.200.195:16379,password=foo1@bared,defaultDatabase=11",
        ApplicationName = "Settlement-LOCAL"
    });

    [Fact]
    public async Task GetAllMessage_ProAndLeagueIdOnCache_ReturnsRightNotification()
    {
        // Arrange
        var list = new List<NotificationModel>
        {
            new ()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new ()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        repository.FetchAsync<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
        const int cacheExpiredInDays = 1095;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));

        await cache.SetAsync(CacheKeys.KeyLeagueId, 1, cachePolicy);

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "PRO"
        };

        var appSettingsOptions = Options.Create(settings);
        var service = new QueryServiceImp(repository, cache, appSettingsOptions);

        // Act
        var actualList = await service.GetAllMessageByEnv();

        // Assert
        Assert.Single(actualList);
    }

    [Fact]
    public async Task GetAllMessage_UatAndLeagueIdOnCache_ReturnsRightNotification()
    {
        // Arrange
        var list = new List<NotificationModel>
        {
            new ()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new ()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        repository.FetchAsync<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
        const int cacheExpiredInDays = 1095;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
        await cache.SetAsync(CacheKeys.KeyLeagueId, 1, cachePolicy);

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "UAT"
        };

        var appSettingsOptions = Options.Create(settings);
        var service = new QueryServiceImp(repository, cache, appSettingsOptions);

        // Act
        var actualList = await service.GetAllMessageByEnv();

        // Assert
        Assert.Single(actualList);
    }

    [Fact]
    public async Task GetAllMessage_LocalAndLeagueIdOnCache_ReturnsRightNotification()
    {
        // Arrange
        var list = new List<NotificationModel>
        {
            new ()
            {
                MatchId = 1,
                LeagueId = 1
            },
            new ()
            {
                MatchId = 2,
                LeagueId = 2
            }
        };

        repository.FetchAsync<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
        const int cacheExpiredInDays = 1095;
        var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
        await cache.SetAsync(CacheKeys.KeyLeagueId, 1, cachePolicy);

        var settings = new AppSettingsKey()
        {
            EnvironmentName = "Local"
        };

        var appSettingsOptions = Options.Create(settings);
        var service = new QueryServiceImp(repository, cache, appSettingsOptions);

        // Act
        var actualList = await service.GetAllMessageByEnv();

        // Assert
        Assert.True(actualList.Count == 2);
    }
}