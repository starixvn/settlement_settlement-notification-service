using Fanex.Logging;
using SettlementNotification.Models;
using System;

namespace SettlementNotification.Services
{
    public interface NotificationService
    {
        void Notify(NotificationModel notificationModel);
    }

    public class NotificationServiceImp : NotificationService
    {
        private readonly RefDataService refDataService;

        private readonly QueryService queryService;

        private readonly SendMessageService sendMessageService;

        private readonly ContentService contentService;

        private readonly NotificationPoolService notificationPoolService;

        public NotificationServiceImp(
            RefDataService refDataService,
            QueryService queryService,
            SendMessageService sendMessageService,
            ContentService contentService,
            NotificationPoolService notificationPoolService)
        {
            this.refDataService = refDataService;
            this.queryService = queryService;
            this.sendMessageService = sendMessageService;
            this.contentService = contentService;
            this.notificationPoolService = notificationPoolService;
        }

        public void Notify(NotificationModel notificationModel)
        {
            try
            {
                notificationModel.RefDataService = refDataService;
                var content = contentService.Content(notificationModel);
                sendMessageService.Send(notificationModel.MessageGroupName, content);
                queryService.UpdateFlag(notificationModel.MessageId);
            }
            catch (Exception ex)
            {
                LogService.Error("Exception in send notifications:", ex);
            }
            finally
            {
                notificationPoolService.Remove(notificationModel.MessageId);
            }
        }
    }
}