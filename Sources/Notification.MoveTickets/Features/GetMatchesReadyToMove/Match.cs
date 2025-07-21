namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public class Match
{
    public int MatchId { get; set; }
    public int SessionTime { get; set; }
    public int GameSession { get; set; }
    public DateTime? KickOffTime { get; set; }
    public short SportType { get; set; }
    public bool IsAutoMove { get; set; }
}
