namespace SettlementNotification.Criteria;

public class GetAllMessageCriteria : CriteriaBase
{
    public override string GetSettingKey() => "NAP_Set_LetstalkMessage_Select";

    public override bool IsValid() => true;
}