using System;
using SettlementNotification.Services;

namespace SettlementNotification.Models;

public class NotificationModel
{
    public RefDataService RefDataService { get; set; }

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
            const int instantSettlement = 1;
            const int settlement = 2;
            return MessageGroupId switch
            {
                instantSettlement => "Instant Settlement",
                settlement => "Settlement",
                _ => string.Empty
            };
        }
    }

    public string MessageType { get; set; }

    public string ActionName { get; set; }

    public DateTime KickOffTime { get; set; }

    public string MessageContent { get; set; }

    public override bool Equals(object obj) =>
        obj is NotificationModel model
        && model.MessageId == MessageId;

    public override int GetHashCode() => MessageId;
}