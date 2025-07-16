using System;

namespace Notification.MoveTickets.Models;

public class MoveTicketInfo
{
    public DateTime EventDate { get; set; }

    public int ChildMatchId { get; set; }

    public string Resources { get; set; }
}
