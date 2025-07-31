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
    }

    public static class ExecutionMessages
    {
        public const string ExecutionTime = "System shall execute move actions in the next 15 minutes (23:45 GMT-4).";
        public const string Unresolved = "Unresolved matches will not be moved.";
        public const string NewMatch = "System shall also move ticket on new match found that eligible to move tickets.";
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
    }

    public static class CacheKeys
    {
        public const string AutoMoveStatus = "Settlement-AutoMoveStatus";
    }
}