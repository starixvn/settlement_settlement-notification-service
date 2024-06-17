using Fanex.Data;

namespace SettlementNotification.Criteria;

public class CheckRunningTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public string ResourceId { get; set; }
    public bool IsExistsRunningTicket { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => "NAP_Set_CheckRunningTicket";

    public override bool IsValid() => true;
}