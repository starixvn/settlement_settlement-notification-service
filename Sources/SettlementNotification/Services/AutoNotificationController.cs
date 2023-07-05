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
        var list = notificationPoolService.PeekNewItem();
        list.ForEach(m =>
        {
            notificationService.Notify(m);
        });
    }
}