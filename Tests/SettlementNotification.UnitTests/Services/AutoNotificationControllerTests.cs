using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System.Collections.Generic;
using Xunit;

namespace SettlementNotification.UnitTests.Services
{
    public class AutoNotificationControllerTests
    {
        [Fact]
        public void Process_HasNotification_InvokeProcessMethod()
        {
            var notificationPoolService = Substitute.For<NotificationPoolService>();
            var notificationService = Substitute.For<NotificationService>();
            var controller = new AutoNotificationControllerImp(notificationPoolService, notificationService);

            // Arrange
            var list = new List<NotificationModel>
            {
                new NotificationModel
                {
                    MatchId = 1
                }
            };

            notificationPoolService.PeekNewItem().Returns(list);

            // Act
            controller.Process();

            // Assert
            notificationService.Received().Notify(list[0]);
        }
    }
}
