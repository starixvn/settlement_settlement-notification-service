namespace SettlementNotification.Criteria;

public class UpdateFlagCriteria : NonQueryCommand
{
    public int MessageId { get; set; }

    public override string GetSettingKey() => "NAP_Set_LetstalkMessage_Update";

    public override bool IsValid() => true;
}