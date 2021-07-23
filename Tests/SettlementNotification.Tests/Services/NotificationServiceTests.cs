using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Core.Services;
using Fanex.RemotingRef;
using Microsoft.Extensions.Options;
using NSubstitute;
using SettlementNotification.Models;
using SettlementNotification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SettlementNotification.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly RefDataService refDataService;

        private readonly QueryService queryService;

        private readonly SendMessageService sendMessageService;

        private readonly NotificationPoolService notificationPoolService;

        private readonly LetstalkClientService letstalkClientService;

        private readonly NotificationService service;

        public NotificationServiceTests()
        {
            AppConfigService.Config.LetstalkServiceApi = "http://letstalk-webapi.playthefun.com/";
            RefDataTypeContainer
              .ConnectTo(new Uri("http://ha.nexdev.net:7258/Main/"))
              .FromApplication("Settlement");

            var remotingRefService = new RemotingRefService();
            refDataService = new RefDataServiceImp(remotingRefService);
            queryService = Substitute.For<QueryService>();
            notificationPoolService = Substitute.For<NotificationPoolService>();
            var contentService = new ContentServiceImp();
            var settings = new AppSettingsKey()
            {
                LetstalkTargetName = "SettlementNotification",
                LetstalkUserGroup = "Settlement-Notification-Test",
                LetstalkTestGroup = "Settlement-Notification-Test",
                EnvironmentName = "LOCAL",
            };
            IOptions<AppSettingsKey> appSettingskey = Options.Create(settings);

            letstalkClientService = new LetstalkClientServiceImp();

            sendMessageService = new SendMessageServiceImp(contentService, letstalkClientService, appSettingskey);

            service = new NotificationServiceImp(refDataService, queryService, sendMessageService, notificationPoolService);
        }

        private NotificationModel Create()
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
                MessageContent = "Kaden.Trinh",
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
}