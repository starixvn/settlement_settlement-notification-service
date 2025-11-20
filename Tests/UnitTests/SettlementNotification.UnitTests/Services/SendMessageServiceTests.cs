using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Client.Models;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Constants;
using SettlementNotification.Models;
using SettlementNotification.Services;
using Xunit;

namespace SettlementNotification.UnitTests.Services;

public class SendMessageServiceTests
{
    private readonly ContentService contentService;
    private readonly LetstalkClientService letstalkClientService;

    public SendMessageServiceTests()
    {
        contentService = Substitute.For<ContentService>();
        letstalkClientService = Substitute.For<LetstalkClientService>();
    }

    [Fact]
    public void Send_ProEnvironmentAndNotTest_ReturnsEmptyEnvironmentName()
    {
        // Arrange
        var settings = CreateAppSettings("PRO", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.Instant
        };

        contentService.Content(notificationModel).Returns("Test Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Subject == "Instant Settlement" &&
            m.Group == "ProdGroup" &&
            m.Content == "Test Content" &&
            m.TargetName == settings.Letstalk.TargetName));
    }

    [Fact]
    public void Send_NonProEnvironment_ReturnsEnvironmentNameWithSpace()
    {
        // Arrange
        var settings = CreateAppSettings("UAT", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.Instant
        };

        contentService.Content(notificationModel).Returns("Test Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Subject == "UAT - Instant Settlement" &&
            m.Group == "TestGroup"));
    }

    [Fact]
    public void Send_ProEnvironmentButIsTest_ReturnsEnvironmentNameWithSpace()
    {
        // Arrange
        var settings = CreateAppSettings("PRO", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = true,
            MessageGroupId = MessageGroup.Instant
        };

        contentService.Content(notificationModel).Returns("Test Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Subject == "PRO - Instant Settlement" &&
            m.Group == "TestGroup"));
    }

    [Fact]
    public void Send_RefundReminderGroupAndProEnvironment_ReturnsUserGroupRefund()
    {
        // Arrange
        var settings = CreateAppSettings("PRO", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.RefundReminder,
            IsTest = false,
            MessageGroupId = MessageGroup.ConfirmRefund
        };

        contentService.Content(notificationModel).Returns("Refund Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "ProdRefundGroup" &&
            m.Subject == "Settlement"));
    }

    [Fact]
    public void Send_RefundReminderGroupAndNonProEnvironment_ReturnsTestGroup()
    {
        // Arrange
        var settings = CreateAppSettings("UAT", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.RefundReminder,
            IsTest = false,
            MessageGroupId = MessageGroup.ConfirmRefund
        };

        contentService.Content(notificationModel).Returns("Refund Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "TestGroup" &&
            m.Subject == "UAT - Settlement"));
    }

    [Fact]
    public void Send_HybridParlayMessageGroupAndProEnvironment_ReturnsUserGroupHybridParlay()
    {
        // Arrange
        var settings = CreateAppSettings("PRO", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.HybridParlay
        };

        contentService.Content(notificationModel).Returns("Hybrid Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "ProdHybridGroup" &&
            m.Subject == "Settlement Bet Builder+"));
    }

    [Fact]
    public void Send_HybridParlayMessageGroupAndNonProEnvironment_ReturnsTestGroup()
    {
        // Arrange
        var settings = CreateAppSettings("UAT", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.HybridParlay
        };

        contentService.Content(notificationModel).Returns("Hybrid Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "TestGroup" &&
            m.Subject == "UAT - Settlement Bet Builder+"));
    }

    [Fact]
    public void Send_NonHybridParlayMessageGroupAndProEnvironment_ReturnsUserGroupNotification()
    {
        // Arrange
        var settings = CreateAppSettings("PRO", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.Instant
        };

        contentService.Content(notificationModel).Returns("Instant Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "ProdGroup" &&
            m.Subject == "Instant Settlement"));
    }

    [Fact]
    public void Send_NonHybridParlayMessageGroupAndNonProEnvironment_ReturnsTestGroup()
    {
        // Arrange
        var settings = CreateAppSettings("SIT", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.FastMarket
        };

        contentService.Content(notificationModel).Returns("Fast Market Content");

        // Act
        service.Send(notificationModel);

        // Assert
        letstalkClientService.Received(1).SendMessage(Arg.Is<LetstalkModel>(m =>
            m.Group == "TestGroup" &&
            m.Subject == "SIT - Settlement"));
    }

    [Fact]
    public void Send_Always_CallsContentServiceAndLetstalkClientService()
    {
        // Arrange
        var settings = CreateAppSettings("LOCAL", "ProdGroup", "ProdRefundGroup", "ProdHybridGroup", "TestGroup");
        var appSettingsOptions = Options.Create(settings);
        var service = new SendMessageServiceImp(contentService, letstalkClientService, appSettingsOptions);

        var notificationModel = new NotificationModel
        {
            LetstalkGroup = LetstalkGroup.Notification,
            IsTest = false,
            MessageGroupId = MessageGroup.Instant
        };

        contentService.Content(notificationModel).Returns("Test Content");

        // Act
        service.Send(notificationModel);

        // Assert
        contentService.Received(1).Content(notificationModel);
        letstalkClientService.Received(1).SendMessage(Arg.Any<LetstalkModel>());
    }

    private static AppSettingsKey CreateAppSettings(
        string environmentName,
        string userGroupNotification,
        string userGroupRefund,
        string userGroupHybridParlay,
        string testGroup)
    {
        return new AppSettingsKey
        {
            EnvironmentName = environmentName,
            Letstalk = new LetstalkSettings
            {
                TargetName = "SettlementNotification",
                UserGroupNotification = userGroupNotification,
                UserGroupRefund = userGroupRefund,
                UserGroupHybridParlay = userGroupHybridParlay,
                TestGroup = testGroup
            }
        };
    }
}