using SettlementNotification.Constants;
using SettlementNotification.Services;

namespace SettlementNotification.Models;

public class NotificationModel
{
    private string actionName = string.Empty;

    private string messageContent = string.Empty;

    public RefDataService RefDataService { get; set; }

    public int LetstalkGroup { get; set; }

    public int MessageId { get; init; }

    public int MatchId { get; set; }

    public int HomeId { get; set; }

    public string HomeName => RefDataService.GetTeamName(HomeId);

    public int AwayId { get; set; }

    public string AwayName => RefDataService.GetTeamName(AwayId);

    public int LeagueId { get; set; }

    public string LeagueName => RefDataService.GetLeagueName(LeagueId);

    public int MessageGroupId { get; set; }

    public bool IsTest { get; set; }

    public string MessageGroupName
    {
        get
        {
            return MessageGroupId switch
            {
                MessageGroup.Instant => "Instant Settlement",
                MessageGroup.FastMarket => "Settlement",
                MessageGroup.ConfirmRefund => "Settlement",
                _ => string.Empty
            };
        }
    }

    public string MessageType { get; set; }

    public string ActionName
    {
        get
        {
            return MessageGroupId switch
            {
                MessageGroup.FastMarket => "Completed match with Auto-Settle FM turned OFF",
                _ => actionName
            };
        }
        set => actionName = value;
    }

    public DateTime KickOffTime { get; set; }

    public DateTime CreatedDate { get; set; }

    public string MessageContent
    {
        get
        {
            return MessageGroupId switch
            {
                MessageGroup.FastMarket => messageContent.Replace("@latest_settle_time", CreatedDate.ToString("MMM dd, yyyy hh:mm:ss tt")),
                _ => messageContent
            };
        }
        set => messageContent = value;
    }

    public override bool Equals(object obj) =>
        obj is NotificationModel model
        && model.MessageId == MessageId;

    public override int GetHashCode() => MessageId;
}