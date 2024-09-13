using SettlementNotification.Constants;

namespace SettlementNotification.Services;

public interface AutoNotificationController
{
    void Process();
}

public class AutoNotificationControllerImp(
    NotificationPoolService notificationPoolService,
    NotificationService notificationService)
    : AutoNotificationController
{
    public void Process()
    {
        var list = notificationPoolService.PeekNewItem();
        list.ForEach(m =>
        {
            m.LetstalkGroup = LetstalkGroup.Notification;
            notificationService.Notify(m);
        });
    }
}