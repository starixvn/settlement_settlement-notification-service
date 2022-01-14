using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using Fanex.Caching;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Models;
using Fanex.Caching.Redis;
using Xunit;
using SettlementNotification.Criteria;
using SettlementNotification.Services;

namespace SettlementNotification.UnitTests.Services
{
    public class QueryServiceTests
    {
        private readonly IDynamicRepository repository;

        private readonly ICache cache;

        public QueryServiceTests()
        {
            repository = Substitute.For<IDynamicRepository>();
            cache = new RedisCache(new RedisCacheOptions
            {
                ConnectionString = $"ha.nexdev.net:16379,password=foo1@bared,defaultDatabase=11",
                ApplicationName = "Settlement-LOCAL"
            });
        }

        [Fact]
        public void GetAllMessage_ProAndLeagueIdOnCache_ReturnsRightNotification()
        {
            // Arrange
            var list = new List<NotificationModel>
            {
                new NotificationModel
                {
                    MatchId = 1,
                    LeagueId = 1
                },
                new NotificationModel
                {
                    MatchId = 2,
                    LeagueId = 2
                }
            };


            repository.Fetch<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
            const string keyIncludeLeagueIds = "Settlement-AutoPayoutOtherSport-LeagueId";
            const int cacheExpiredInDays = 1095;
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
            cache.Set(keyIncludeLeagueIds, 1, cachePolicy);

            var settings = new AppSettingsKey()
            {
                EnvironmentName = "PRO"
            };

            var appSettingsOptions = Options.Create(settings);
            var service = new QueryServiceImp(repository, cache, appSettingsOptions);

            // Act
            var actualList = service.GetAllMessage();

            // Assert
            Assert.Single(actualList);
        }

        [Fact]
        public void GetAllMessage_UatAndLeagueIdOnCache_ReturnsRightNotification()
        {
            // Arrange
            var list = new List<NotificationModel>
            {
                new NotificationModel
                {
                    MatchId = 1,
                    LeagueId = 1
                },
                new NotificationModel
                {
                    MatchId = 2,
                    LeagueId = 2
                }
            };


            repository.Fetch<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
            const string keyIncludeLeagueIds = "Settlement-AutoPayoutOtherSport-LeagueId";
            const int cacheExpiredInDays = 1095;
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
            cache.Set(keyIncludeLeagueIds, 1, cachePolicy);

            var settings = new AppSettingsKey()
            {
                EnvironmentName = "UAT"
            };

            var appSettingsOptions = Options.Create(settings);
            var service = new QueryServiceImp(repository, cache, appSettingsOptions);

            // Act
            var actualList = service.GetAllMessage();

            // Assert
            Assert.Single(actualList);
        }

        [Fact]
        public void GetAllMessage_LocalAndLeagueIdOnCache_ReturnsRightNotification()
        {
            // Arrange
            var list = new List<NotificationModel>
            {
                new NotificationModel
                {
                    MatchId = 1,
                    LeagueId = 1
                },
                new NotificationModel
                {
                    MatchId = 2,
                    LeagueId = 2
                }
            };


            repository.Fetch<NotificationModel>(Arg.Any<GetAllMessageCriteria>()).Returns(list);
            const string keyIncludeLeagueIds = "Settlement-AutoPayoutOtherSport-LeagueId";
            const int cacheExpiredInDays = 1095;
            var cachePolicy = new CacheItemOptions().SetAbsoluteExpiration(TimeSpan.FromDays(cacheExpiredInDays));
            cache.Set(keyIncludeLeagueIds, 1, cachePolicy);

            var settings = new AppSettingsKey()
            {
                EnvironmentName = "Local"
            };

            var appSettingsOptions = Options.Create(settings);
            var service = new QueryServiceImp(repository, cache, appSettingsOptions);

            // Act
            var actualList = service.GetAllMessage();

            // Assert
            Assert.True(actualList.Count == 2);
        }
    }
}
