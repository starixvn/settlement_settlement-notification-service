using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SettlementNotification.UnitTests.Services;

public class NotificationPoolServiceTests
{
    private readonly QueryService queryService = Substitute.For<QueryService>();

    private static List<NotificationModel> CreateMessage(params int[] messageIds)
    {
        return messageIds.Select(i => new NotificationModel { MessageId = i }).ToList();
    }

    [Fact]
    public async Task PeekNewItem_Twice_OnlyAddNewMessageToPool()
    {
        var service = new NotificationPoolServiceImp(queryService);
        queryService.GetAllMessageByEnv().Returns(CreateMessage(1, 3));
        await service.PeekNewItem();

        queryService.GetAllMessageByEnv().Returns(CreateMessage(2, 3));
        var items = await service.PeekNewItem();

        Assert.Equal(3, service.PoolOfNotification.Count);
        Assert.Single(items);
    }

    [Fact]
    public async Task Remove_ExistedMessageId_ReturnOneNotificationOnPool()
    {
        // Arrange
        var messages = CreateMessage(1, 2);
        var service = new NotificationPoolServiceImp(queryService);
        queryService.GetAllMessageByEnv().Returns(messages);

        // Act
        await service.PeekNewItem();
        service.Remove(1);

        // Assert
        Assert.Single(service.PoolOfNotification);
    }
}