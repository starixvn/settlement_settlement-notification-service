using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Models;
using SettlementNotification.Constants;
using System.Text;

namespace Notification.MoveTickets.Services;

public interface INotificationContentService
{
    string CreateNotificationContent(List<Match> matches, bool isAutoMoveEnabled);

    string CreateExecutionSummaryContent(List<Match> matchesSuccess, List<Match> matchesFail);
}

public class NotificationContentService : INotificationContentService
{
    public string CreateExecutionSummaryContent(List<Match> matchesSuccess, List<Match> matchesFail)
    {
        var contentBuilder = new StringBuilder();
        var date = DateTimeOffset.Now;
        var offsetHours = date.Offset.Hours;
        string executeTime = date.ToString("MMM dd, yyyy HH:mm:ss") + $" GMT{offsetHours:+#;-#;+0}";

        contentBuilder.AppendLine(string.Format(MoveTicketsMessages.ContentTemplates.ExecutionSummaryHeader, executeTime));

        if (matchesSuccess.Count > 0)
        {
            contentBuilder.AppendLine(MoveTicketsMessages.ContentTemplates.SuccessfulMovesHeader);
            AppendSportMatches(contentBuilder, "Baseball", GetMatchesBySport(matchesSuccess, SportType.Baseball));
            AppendSportMatches(contentBuilder, "Cricket", GetMatchesBySport(matchesSuccess, SportType.Cricket));
        }

        if (matchesFail.Count > 0)
        {
            contentBuilder.AppendLine(MoveTicketsMessages.ContentTemplates.FailedMovesHeader);
            AppendSportMatches(contentBuilder, "Baseball", GetMatchesBySport(matchesFail, SportType.Baseball));
            AppendSportMatches(contentBuilder, "Cricket", GetMatchesBySport(matchesFail, SportType.Cricket));
        }

        return contentBuilder.ToString();
    }

    public string CreateNotificationContent(List<Match> matches, bool isAutoMoveEnabled)
    {
        var matchCategories = CategorizeMatches(matches);
        return BuildNotificationContent(isAutoMoveEnabled, matchCategories);
    }

    private static MatchCategories CategorizeMatches(List<Match> matches)
    {
        return new MatchCategories
        {
            BaseballMatches = GetReadyMatchesBySportType(matches, SportType.Baseball),
            CricketMatches = GetReadyMatchesBySportType(matches, SportType.Cricket),
            OnHoldMatches = matches.Where(match => IsMatchOnHold(match.Remark)).ToList(),
            MatchesWithoutChild = GetMatchesWithoutChildMatches(matches)
        };
    }

    private static List<Match> GetReadyMatchesBySportType(List<Match> matches, short sportType)
        => [.. matches.Where(match => match.SportType == sportType && !IsMatchOnHold(match.Remark) && HasValidChildMatch(match.ChildMatchId))];

    private static List<Match> GetMatchesBySport(List<Match> matches, short sportType)
    => [.. matches.Where(match => match.SportType == sportType)];

    private static bool IsMatchOnHold(RemarkSettingModel remark)
    {
        var isPendingOrUndecided = remark.IsPending || remark.IsUndecided;
        var isUnderSettleOrMtdHold = remark.IsUnderSettle || remark.IsMtdHold;

        return isPendingOrUndecided || isUnderSettleOrMtdHold || remark.IsKeyHold;
    }

    private static List<Match> GetMatchesWithoutChildMatches(List<Match> matches)
    {
        return matches.Where(match => !HasValidChildMatch(match.ChildMatchId)).ToList();
    }

    private static bool HasValidChildMatch(int? childMatchId)
    {
        return childMatchId is > 0;
    }

    private static string BuildNotificationContent(bool isAutoMoveEnabled, MatchCategories categories)
    {
        var contentBuilder = new StringBuilder();

        AppendServiceStatus(contentBuilder, isAutoMoveEnabled);
        AppendEligibleMatchesSection(contentBuilder, categories);
        AppendMatchesNeedResolveSection(contentBuilder, categories);
        AppendExecutionInfo(contentBuilder);

        return contentBuilder.ToString();
    }

    private static void AppendServiceStatus(StringBuilder builder, bool isAutoMoveEnabled)
    {
        var status = isAutoMoveEnabled ? "ON" : "OFF";
        builder.AppendLine(string.Format(MoveTicketsMessages.ContentTemplates.ServiceStatusTemplate, status));
        builder.AppendLine(MoveTicketsMessages.ContentTemplates.EligibleMatchesHeader);
    }

    private static void AppendEligibleMatchesSection(StringBuilder builder, MatchCategories categories)
    {
        AppendSportMatches(builder, "Baseball", categories.BaseballMatches);
        AppendSportMatches(builder, "Cricket", categories.CricketMatches);
    }

    private static void AppendSportMatches(StringBuilder builder, string sportName, List<Match> matches)
    {
        if (matches.Any())
        {
            var matchIds = string.Join(", ", matches.Select(match => match.MatchId));
            builder.AppendLine(string.Format(MoveTicketsMessages.ContentTemplates.SportMatchesTemplate, sportName, matchIds));
        }
    }

    private static void AppendMatchesNeedResolveSection(StringBuilder builder, MatchCategories categories)
    {
        builder.AppendLine();
        builder.AppendLine(MoveTicketsMessages.ContentTemplates.MatchesNeedResolveHeader);

        AppendResolveCategory(builder, "matches on-hold", categories.OnHoldMatches);
        AppendResolveCategory(builder, "matches without corresponding child-match to move tickets", categories.MatchesWithoutChild);
    }

    private static void AppendResolveCategory(StringBuilder builder, string categoryName, List<Match> matches)
    {
        if (matches.Any())
        {
            var matchIds = string.Join(", ", matches.Select(match => match.MatchId));
            builder.AppendLine(string.Format(MoveTicketsMessages.ContentTemplates.ResolveCategoryTemplate, categoryName, matchIds));
        }
    }

    private static void AppendExecutionInfo(StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine(MoveTicketsMessages.ExecutionMessages.ExecutionTime);
        builder.AppendLine(MoveTicketsMessages.ExecutionMessages.Unresolved);
        builder.AppendLine(MoveTicketsMessages.ExecutionMessages.NewMatch);
    }
}

public class MatchCategories
{
    public List<Match> BaseballMatches { get; set; } = [];
    public List<Match> CricketMatches { get; set; } = [];
    public List<Match> OnHoldMatches { get; set; } = [];
    public List<Match> MatchesWithoutChild { get; set; } = [];
}