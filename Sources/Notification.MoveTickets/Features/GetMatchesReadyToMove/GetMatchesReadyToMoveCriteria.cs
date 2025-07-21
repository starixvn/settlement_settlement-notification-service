using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public class GetMatchesReadyToMoveCriteria : CriteriaBase
{
    public override string GetSettingKey()
    {
        return "NAP_Admin_Settlement_GetMatchesReadyToMove";
    }

    public override bool IsValid()
    {
        return true;
    }
}
