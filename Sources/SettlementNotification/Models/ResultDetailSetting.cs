namespace SettlementNotification.Models;

public class ResultDetailSetting
{
    private readonly byte confirmRefundPosition = 2;
    private readonly byte confirmedRefundSdtPosition = 3;

    public byte ResultSetting { get; set; }

    public bool IsConfirmRefund => IsBitSet(ResultSetting, confirmRefundPosition);
    public bool IsConfirmedRefundSdt => IsBitSet(ResultSetting, confirmedRefundSdtPosition);

    public ResultDetailSetting(byte resultSetting)
    {
        ResultSetting = resultSetting;
    }

    private static bool IsBitSet(byte value, int position) => (value & (1 << (position - 1))) != 0;
}