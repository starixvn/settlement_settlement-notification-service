using SettlementNotification.Criteria;
using System.Threading.Tasks;

namespace SettlementNotification.Services;

public interface ISettleLogService
{
    Task WriteLog(int matchId, string message, int actionId);
}

public class SettleLogService(IDynamicRepository repository) : ISettleLogService
{
    public async Task WriteLog(int matchId, string message, int actionId)
    {
        var criteria = new SettleLogCriteria
        {
            MatchId = matchId,
            Remark = message,
            ActionId = actionId,
        };

        await repository.ExecuteAsync(criteria);
    }
}
