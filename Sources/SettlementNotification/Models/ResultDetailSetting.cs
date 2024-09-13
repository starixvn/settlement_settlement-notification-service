namespace SettlementNotification.Models;

public class ResultDetailSetting(byte resultSetting)
{
    private const byte confirmRefundPosition = 2;
    private const byte confirmedRefundSdtPosition = 3;

    public byte ResultSetting { get; set; } = resultSetting;

    public bool IsConfirmRefund => IsBitSet(ResultSetting, confirmRefundPosition);
    public bool IsConfirmedRefundSdt => IsBitSet(ResultSetting, confirmedRefundSdtPosition);

    private static bool IsBitSet(byte value, int position) => (value & (1 << (position - 1))) != 0;
}