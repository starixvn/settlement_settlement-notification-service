using SettlementNotification.Models;
using System;

namespace SettlementNotification.Services;

public interface AutoNotificationController
{
    void Process();
}

public class AutoNotificationControllerImp : AutoNotificationController
{
    private readonly NotificationPoolService notificationPoolService;

    private readonly NotificationService notificationService;

    public AutoNotificationControllerImp(
        NotificationPoolService notificationPoolService,
        NotificationService notificationService)
    {
        this.notificationPoolService = notificationPoolService;
        this.notificationService = notificationService;
    }

    public void Process()
    {
        var model = new NotificationModel
        {
            MessageId = 9999,
            MatchId = 8888,
            HomeId = 181,
            AwayId = 250,
            IsTest = true,
            KickOffTime = DateTime.Now,
            MessageGroupId = 1,
            MessageContent = "message",
            ActionName = "Auto Test",
            MessageType = "Warning"
        };
        notificationService.Notify(model);
        //var list = notificationPoolService.PeekNewItem();
        //list.ForEach(m =>
        //{
        //    notificationService.Notify(m);
        //});
    }
}