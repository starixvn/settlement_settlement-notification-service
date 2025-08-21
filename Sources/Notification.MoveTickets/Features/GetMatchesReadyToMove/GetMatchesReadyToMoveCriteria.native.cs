using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public class GetMatchesReadyToMoveCriteria : CriteriaBase
{
    public DateTime EventDate { get; set; } = DateTimeOffset.Now.Date;

    public override string GetSettingKey() => "NAP_Admin_Settlement_AutoMove_MatchInfo";

    public override bool IsValid() => true;
}