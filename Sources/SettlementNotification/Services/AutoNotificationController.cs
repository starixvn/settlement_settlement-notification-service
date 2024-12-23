using SettlementNotification.Constants;
using System.Threading.Tasks;

namespace SettlementNotification.Services;

public interface AutoNotificationController
{
    Task Process();
}

public class AutoNotificationControllerImp(
    NotificationPoolService notificationPoolService,
    NotificationService notificationService)
    : AutoNotificationController
{
    public async Task Process()
    {
        var list = await notificationPoolService.PeekNewItem();
        list.ForEach(m =>
        {
            m.LetstalkGroup = LetstalkGroup.Notification;
            notificationService.Notify(m);
        });
    }
}