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
        list.ForEach(notificationService.Notify);
    }
}