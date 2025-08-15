namespace SettlementNotification.Models;

public class RemarkSettings
{
    private readonly byte confirmRefundAllPosition = 6;
    public bool IsConfirmRefundAll => IsBitSet(SettlementStatus, confirmRefundAllPosition);

    public byte SettlementStatus { get; set; }

    public RemarkSettings(byte settlementStatus)
    {
        SettlementStatus = settlementStatus;
    }

    private static bool IsBitSet(byte value, int position) => (value & (1 << (position - 1))) != 0;
}