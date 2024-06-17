namespace SettlementNotification.Models;

public class ConfirmRefundKeyInfo
{
    private const string KeyConfirmRefund = "SettlementConfirmRefund";
    public int MatchId { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;

    public bool IsValid => Prefix.Equals(KeyConfirmRefund) && MatchId > 0;

    public ConfirmRefundKeyInfo(string key)
    {
        var keyInfo = !string.IsNullOrEmpty(key) ? key.Split(':') : [];

        if (keyInfo.Length > 2)
        {
            Prefix = keyInfo[0];
            MatchId = Convert.ToInt32(keyInfo[1]);
            ResourceId = keyInfo[2];
        }
    }
}