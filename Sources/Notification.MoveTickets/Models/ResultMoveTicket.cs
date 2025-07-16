using System;

namespace Notification.MoveTickets.Models;

public class ResultMoveTicket
{
    public string ErrMsg { get; set; } = string.Empty;

    public int ErrCode { get; set; }

    public int TicketCount { get; set; }
}
