namespace Notification.MoveTickets.Constants;

public static class MoveTicketsMessages
{
    public static class ActionNames
    {
        public const string AutoMoveTickets = "Auto move tickets of settled results";
    }

    public static class MessageTypes
    {
        public const string Reminder = "Reminder";
        public const string Summary = "Summary";
    }

    public static class ExecutionMessages
    {
        public const string ExecutionTime = "System shall execute move actions in the next 15 minutes (23:45 GMT-4).";
        public const string Unresolved = "Unresolved matches will not be moved.";
        public const string NewMatch = "System shall also move tickets for any newly found match that is eligible for tickets transfer.";
    }

    public static class ContentTemplates
    {
        public const string ServiceStatusTemplate = "Auto move tickets of settled results service = {0}.";
        public const string EligibleMatchesHeader = "System detect following matches eligible for moving tickets of settled results:";
        public const string MatchesNeedResolveHeader = "Matches need resolve:";
        public const string SportMatchesTemplate = "+ {0}: {1}";
        public const string ResolveCategoryTemplate = "+ {0}: {1}";
        public const string SettingStatusOffTemplate = "Auto move tickets has not been executed due to Auto Move Tickets Setting status is OFF.";
        public const string NoMatchEligible = "System detect 0 match eligible to move tickets.";
        public const string NoMatchMovie = "Auto move tickets has not been executed due to no match is quailfied to move.";

        // New execution summary templates
        public const string ExecutionSummaryHeader = "Auto move tickets of settled results service has been executed on {0}.";
        public const string SuccessfulMovesHeader = "Successful moving tickets for:";
        public const string FailedMovesHeader = "Failed moving tickets for:";
        public const string SportSuccessTemplate = "+ {0}: {1}";
        public const string SportFailureTemplate = "+ {0}: {1}";
        public const string NoSuccessfulMoves = "No matches were successfully moved.";
        public const string NoFailedMoves = "All matches were processed successfully.";
        public const string ExecutionSummaryFooter = "Auto move tickets execution completed.";
    }

    public static class CacheKeys
    {
        public const string AutoMoveStatus = "Settlement-AutoMoveStatus";
    }

    public static class SportNames
    {
        public const string Soccer = "Soccer";
        public const string Baseball = "Baseball";
        public const string Cricket = "Cricket";
        public const string Basketball = "Basketball";
        public const string Tennis = "Tennis";
        public const string Football = "Football";
        public const string Hockey = "Hockey";
        public const string Volleyball = "Volleyball";
        public const string Rugby = "Rugby";
        public const string Golf = "Golf";
    }

    public static class ExecutionStatus
    {
        public const string Success = "SUCCESS";
        public const string Failed = "FAILED";
        public const string Pending = "PENDING";
        public const string InProgress = "IN_PROGRESS";
    }
}