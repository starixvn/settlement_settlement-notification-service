using SettlementNotification.Services;

namespace SettlementNotification.Models;

public class MatchDetail
{
    public RefDataService RefDataService { get; set; }

    public int MatchId { get; set; }

    public string MatchCode { get; set; }

    public DateTime KickOffTime { get; set; }

    public int HomeId { get; set; }

    public int AwayId { get; set; }

    public string HomeName => RefDataService.GetTeamName(HomeId);

    public string AwayName => RefDataService.GetTeamName(AwayId);

    public int LeagueId { get; set; }

    public string LeagueName => RefDataService.GetLeagueName(LeagueId);

    public short ChildMatchType { get; set; }

    public int SportType { get; set; }
    public int ParentId { get; set; }

    public byte SettlementStatus { get; set; }
    public string BetStatus { get; set; }
    public string EventStatus { get; set; }

    public RemarkSettings RemarkSettings => new(SettlementStatus);
}