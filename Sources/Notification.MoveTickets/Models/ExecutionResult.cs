using Notification.MoveTickets.Constants;

namespace Notification.MoveTickets.Models;

public class ExecutionResult
{
    public DateTime ExecutionTime { get; set; }
    public List<Match> SuccessfulMatches { get; set; } = [];
    public List<Match> FailedMatches { get; set; } = [];
    public int TotalSuccessfulTickets { get; set; }
    public int TotalFailedTickets { get; set; }
    public ExecutionStatus Status { get; set; }
    public string SummaryMessage { get; set; } = string.Empty;
    
    public bool HasSuccessfulMoves => SuccessfulMatches?.Any() == true;
    public bool HasFailedMoves => FailedMatches?.Any() == true;
    public bool IsCompleteSuccess => HasSuccessfulMoves && !HasFailedMoves;
    public bool IsCompleteFailure => !HasSuccessfulMoves && HasFailedMoves;
    public bool IsPartialSuccess => HasSuccessfulMoves && HasFailedMoves;
    
    public int TotalMatchesProcessed => (SuccessfulMatches?.Count ?? 0) + (FailedMatches?.Count ?? 0);
    public int TotalTicketsProcessed => TotalSuccessfulTickets + TotalFailedTickets;
}

public enum ExecutionStatus
{
    Pending,
    InProgress,
    Success,
    PartialSuccess,
    Failed,
    Cancelled
}
