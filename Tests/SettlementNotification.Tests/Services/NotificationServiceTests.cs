using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Core.Services;
using Fanex.Logging;
using Fanex.RemotingRef;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System;
using Xunit;

namespace SettlementNotification.Tests.Services;

public class NotificationServiceTests
{
    private readonly NotificationService service;

    public NotificationServiceTests()
    {
        AppConfigService.Config.LetstalkServiceApi = "http://napmailletstalk.nexdev.net/";
        RefDataTypeContainer
            .ConnectTo(new Uri("http://remotingref-api.nexdev.net/main/"))
            .FromApplication("Settlement");

        var remotingRefService = new RemotingRefService();
        var refDataService = new RefDataServiceImp(remotingRefService);
        var queryService = Substitute.For<QueryService>();
        var notificationPoolService = Substitute.For<NotificationPoolService>();
        var contentService = new ContentServiceImp();
        var loggerService = Substitute.For<ILogger>();
        var settings = new AppSettingsKey()
        {
            LetstalkTargetName = "daily-house-wl",
            LetstalkUserGroup = "DWHL-UAT",
            LetstalkTestGroup = "DWHL-UAT",
            EnvironmentName = "LOCAL",
        };
        var appSettingsKey = Options.Create(settings);

        var letstalkClientService = new LetstalkClientServiceImp();

        var sendMessageService = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsKey);

        service = new NotificationServiceImp(refDataService, queryService, sendMessageService, notificationPoolService, loggerService);
    }

    private static NotificationModel Create()
    {
        return new NotificationModel
        {
            MessageId = 9999,
            MatchId = 8888,
            HomeId = 181,
            AwayId = 250,
            IsTest = true,
            KickOffTime = DateTime.Now,
            MessageGroupId = 1,
            MessageContent = "Kaden test message",
            ActionName = "Auto Test",
            MessageType = "Warning"
        };
    }

    [Fact]
    public void Notify_Always_ReturnSendAlert()
    {
        // Arrange
        var model = Create();

        // Act
        service.Notify(model);
    }
}