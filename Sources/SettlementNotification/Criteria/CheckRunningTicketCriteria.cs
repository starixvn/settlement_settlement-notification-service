using Fanex.Data;

namespace SettlementNotification.Criteria;

public class CheckRunningTicketCriteria : NonQueryCommand
{
    public int MatchId { get; set; }
    public short SportType { get; set; }
    public string ResourceIds { get; set; }
    public bool IsExistsTicket { get; set; }

    [SpParam(Ignored = true)]
    public string DbServer { get; set; }

    public override string GetSettingKey() => $"NAP_Set_CheckRunningTicket_ByResourceID_{DbServer}";

    public override bool IsValid() => true;
}