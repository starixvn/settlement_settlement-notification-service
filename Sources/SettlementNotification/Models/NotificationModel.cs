using SettlementNotification.Services;
using System;

namespace SettlementNotification.Models
{
    public class NotificationModel
    {
        public RefDataService RefDataService { get; set; }

        public int MessageId { get; set; }

        public int MatchId { get; set; }

        public int HomeId { get; set; }

        public string HomeName => RefDataService.GetTeamName(HomeId);

        public int AwayId { get; set; }

        public string AwayName => RefDataService.GetTeamName(AwayId);

        public int LeagueId { get; set; }

        public string LeagueName => RefDataService.GetLeagueName(LeagueId);

        public int MessageGroupId { get; set; }

        public string MessageGroupName
        {
            get
            {
                return MessageGroupId == 1 ? "Instant Settlement" : string.Empty;
            }
        }

        public string MessageType { get; set; }

        public string ActionName { get; set; }

        public DateTime KickOffTime { get; set; }

        public string MessageContent { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null
                && (obj is NotificationModel)
                && (obj as NotificationModel).MessageId == MessageId;
        }

        public override int GetHashCode()
        {
            return MessageId;
        }
    }
}