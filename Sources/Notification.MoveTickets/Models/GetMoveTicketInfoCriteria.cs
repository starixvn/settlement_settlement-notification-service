using System;
using Fanex.Data.Repository;

namespace Notification.MoveTickets.Models;
public class GetMoveTicketInfoCriteria : CriteriaBase
{
    public int MatchId { get; set; }

    public override string GetSettingKey()
    {
        return "NAP_Admin_Settlement_50_MoveTicketInfo";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}