namespace SettlementNotification.Services;

using Models;

public interface NotificationPoolService
{
    List<NotificationModel> PeekNewItem();

    void Remove(int id);
}

public class NotificationPoolServiceImp : NotificationPoolService
{
    private readonly QueryService queryService;

    private readonly object lockObject;

    public NotificationPoolServiceImp(QueryService queryService)
    {
        this.queryService = queryService;
        PoolOfNotification = new List<NotificationModel>();
        lockObject = new object();
    }

    public List<NotificationModel> PoolOfNotification { get; set; }

    public List<NotificationModel> PeekNewItem()
    {
        var list = queryService.GetAllMessageByEnv();

        lock (lockObject)
        {
            var newNotification = list.Except(PoolOfNotification).ToList();
            PoolOfNotification.AddRange(newNotification);

            return newNotification;
        }
    }

    public void Remove(int id)
    {
        lock (lockObject)
        {
            PoolOfNotification.RemoveAll(m => m.MessageId == id);
        }
    }
}