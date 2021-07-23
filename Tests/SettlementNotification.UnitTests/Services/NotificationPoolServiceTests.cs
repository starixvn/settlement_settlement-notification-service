using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SettlementNotification.UnitTests.Services
{
    public class NotificationPoolServiceTests
    {
        private readonly QueryService queryService;

        public NotificationPoolServiceTests()
        {
            queryService = Substitute.For<QueryService>();
        }

        private List<NotificationModel> CreateMessage(params int[] messageIds)
        {
            return messageIds.Select(i => new NotificationModel { MessageId = i }).ToList();
        }

        [Fact]
        public void PeekNewItem_Twice_OnlyAddNewMessageToPool()
        {
            var service = new NotificationPoolServiceImp(queryService);
            queryService.GetAllMessage().Returns(CreateMessage(1, 3));
            service.PeekNewItem();

            queryService.GetAllMessage().Returns(CreateMessage(2, 3));
            var items = service.PeekNewItem();

            Assert.Equal(3, service.PoolOfNotification.Count);
            Assert.Single(items);
        }

        [Fact]
        public void Remove_ExistedMessageId_ReturnOneNotificationOnPool()
        {
            // Arrange
            var messages = CreateMessage(1, 2);
            var service = new NotificationPoolServiceImp(queryService);
            queryService.GetAllMessage().Returns(messages);

            // Act
            service.PeekNewItem();
            service.Remove(1);

            // Assert
            Assert.Single(service.PoolOfNotification);
        }
    }
}