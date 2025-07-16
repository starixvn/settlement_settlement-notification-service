using System;
using Fanex.Data;
using Fanex.Data.Repository;

namespace Notification.MoveTickets.Models;

public class MoveTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public int AdminId { get; set; }

    public string Ip { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }

    public DateTime EventDate { get; set; }

    public int ChildMatchId { get; set; }

    public string Resources { get; set; }

    public int TicketCount { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Admin_Settlement_50_MoveTicket_{DbServer}";

    public override bool IsValid() => AdminId > 0 && MatchId > 0;
}