namespace Notification.MoveTickets.Features.MoveTicket;

public class ResultMoveTicket
{
    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}
