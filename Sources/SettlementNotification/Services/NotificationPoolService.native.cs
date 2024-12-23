using System.Threading.Tasks;

namespace SettlementNotification.Services;

using Models;

public interface NotificationPoolService
{
    Task<List<NotificationModel>> PeekNewItem();

    void Remove(int id);
}

public class NotificationPoolServiceImp(QueryService queryService) : NotificationPoolService
{
    private readonly object lockObject = new();

    public List<NotificationModel> PoolOfNotification { get; set; } = [];

    public async Task<List<NotificationModel>> PeekNewItem()
    {
        var list = await queryService.GetAllMessageByEnv();

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