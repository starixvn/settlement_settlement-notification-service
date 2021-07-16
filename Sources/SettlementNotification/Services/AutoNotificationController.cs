using System.Threading.Tasks;

namespace SettlementNotification.Services
{
    public interface AutoNotificationController
    {
        void ProcessAsync();
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

        public void ProcessAsync()
        {
            var list = notificationPoolService.PeekNewItem();
            list.ForEach(m =>
            {
                Task.Run(() => notificationService.Notify(m))
                    .ConfigureAwait(false);
            });
        }
    }
}