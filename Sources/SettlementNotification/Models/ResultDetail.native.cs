namespace SettlementNotification.Models;

public class ResultDetail
{
    public int? MatchId { get; set; }

    public string ResourceId { get; set; }

    public string HomeValue { get; set; }

    public string AwayValue { get; set; }

    public byte ResultSetting { get; set; }
    public byte ResultStatusId { get; set; }
    public byte SettlementStatusId { get; set; }

    public ResultDetailSetting ResultDetailSetting => new(ResultSetting);
}