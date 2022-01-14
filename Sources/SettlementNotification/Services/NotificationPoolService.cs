using SettlementNotification.Models;
using System.Collections.Generic;
using System.Linq;

namespace SettlementNotification.Services
{
    public interface NotificationPoolService
    {
        List<NotificationModel> PeekNewItem();

        void Remove(int id);
    }

    public class NotificationPoolServiceImp : NotificationPoolService
    {
        private readonly QueryService queryService;

        private readonly List<NotificationModel> poolOfNotification;

        private readonly object lockObject;

        public NotificationPoolServiceImp(QueryService queryService)
        {
            this.queryService = queryService;
            poolOfNotification = new List<NotificationModel>();
            lockObject = new object();
        }

        public List<NotificationModel> PoolOfNotification => poolOfNotification;
        

        public List<NotificationModel> PeekNewItem()
        {
            var list = queryService.GetAllMessage();

            lock (lockObject)
            {
                var newNotification = list.Except(poolOfNotification).ToList();
                poolOfNotification.AddRange(newNotification);

                return newNotification;
            }
        }

        public void Remove(int id)
        {
            lock (lockObject)
            {
                poolOfNotification.RemoveAll(m => m.MessageId == id);
            }
        }
    }
}