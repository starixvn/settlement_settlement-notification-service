using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Constants;
using SettlementNotification.Criteria;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SettlementNotification.UnitTests.Services;

public class QueryServiceTests
{
    private readonly IDynamicRepository repository = Substitute.For<IDynamicRepository>();
    private readonly ICacheService cache = Substitute.For<ICacheService>();

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

        // Mock cache to return league ID 1
        cache.TryGetAsync<int>(CacheKeys.KeyLeagueId).Returns((true, 1));

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

        // Mock cache to return league ID 1
        cache.TryGetAsync<int>(CacheKeys.KeyLeagueId).Returns((true, 1));

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

        // Mock cache to return league ID 1
        cache.TryGetAsync<int>(CacheKeys.KeyLeagueId).Returns((true, 1));

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