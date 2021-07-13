using SettlementNotification.Models;
using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Core.Models;
using Microsoft.Extensions.Options;

namespace SettlementNotification.Services
{
    public interface SendMessageService
    {
        void Send(NotificationModel notificationModel);
    }

    public class SendMessageServiceImp : SendMessageService
    {
        private readonly AppSettingsKey appSettingskey;

        private readonly ContentService contentService;

        private readonly LetstalkClientService letstalkClientService;

        public SendMessageServiceImp(ContentService contentService, LetstalkClientService letstalkClientService, IOptions<AppSettingsKey> appSettingskey)
        {
            this.letstalkClientService = letstalkClientService;
            this.appSettingskey = appSettingskey.Value;
            this.contentService = contentService;
        }

        public void Send(NotificationModel notificationModel)
        {
            var content = contentService.Content(notificationModel);
            var letstalkGroupName = appSettingskey.EnvironmentName.Equals("PRO") && !notificationModel.IsTest
                  ? appSettingskey.LetstalkUserGroup
                  : appSettingskey.LetstalkTestGroup;

            var messageModel = CreateLetstalkMessageModel(letstalkGroupName, notificationModel.MessageGroupName, content);
            letstalkClientService.SendMessage(messageModel);
        }

        private LetstalkModel CreateLetstalkMessageModel(string letstalkGroupName, string subject, string content)
        {
            return new LetstalkModel
            {
                TargetName = appSettingskey.LetstalkTargetName,
                Subject = subject,
                Content = content,
                Group = letstalkGroupName
            };
        }
    }
}