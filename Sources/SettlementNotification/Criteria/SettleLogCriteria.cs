namespace SettlementNotification.Criteria;

public class SettleLogCriteria : NonQueryCommand
{
    public int UserId { get; set; }

    public int? MatchId { get; set; }

    public string Remark { get; set; }
    public string RemarkJson { get; set; }

    public string Ip { get; set; } = string.Empty;

    public int ActionId { get; set; }
    public override string GetSettingKey()
        => "WriteLog_UserLog_Settle";

    public override bool IsValid() => true;
}
