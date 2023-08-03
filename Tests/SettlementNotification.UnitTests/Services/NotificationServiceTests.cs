using Fanex.Logging;
using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using Xunit;

namespace SettlementNotification.UnitTests.Services;

public class NotificationServiceTests
{
    private readonly QueryService queryService;

    private readonly SendMessageService sendMessageService;

    private readonly NotificationPoolService notificationPoolService;

    private readonly NotificationService service;

    public NotificationServiceTests()
    {
        var refDataService = Substitute.For<RefDataService>();

        queryService = Substitute.For<QueryService>();
        sendMessageService = Substitute.For<SendMessageService>();
        notificationPoolService = Substitute.For<NotificationPoolService>();
        var loggerService = Substitute.For<ILogger>();
        service = new NotificationServiceImp(refDataService, queryService, sendMessageService, notificationPoolService, loggerService);
    }

    [Fact]
    public void Notify_Always_CallAllMethods()
    {
        // Arrange
        var model = new NotificationModel();

        // Act
        service.Notify(model);

        // Assert
        sendMessageService.Received(1).Send(model);
        queryService.Received(1).UpdateFlag(model.MessageId);
        notificationPoolService.Received(1).Remove(model.MessageId);
    }
}