namespace SettlementNotification.Criteria;

public class GetResultInfoCriteria : CriteriaBase
{
    public int MatchId { get; set; }
    public string ResourceIds { get; set; }

    public override string GetSettingKey() => "NAP_Set_GetResultInfo";

    public override bool IsValid() => MatchId > 0 && !string.IsNullOrEmpty(ResourceIds);
}