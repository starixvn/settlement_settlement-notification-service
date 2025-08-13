namespace Notification.MoveTickets.Models;

public class Match
{
    public int MatchId { get; set; }
    public int SessionTime { get; set; }
    public int GameSession { get; set; }
    public DateTime? KickOffTime { get; set; }
    public short SportType { get; set; }
    public byte SettlementStatus { get; set; }
    public int? ChildMatchId { get; set; }
    public bool IsTest { get; set; }
    public string ResourceId { get; set; } = string.Empty;

    public List<string> ResourceIds { get; set; } = [];

    public RemarkSettingModel Remark => new(SettlementStatus);
}

public class RemarkSettingModel
{
    private const int BinaryBase = 2;
    private const char ActiveFlag = '1';

    private const int PendingBitPosition = 0;
    private const int UndecidedBitPosition = 1;
    private const int UnderSettleBitPosition = 2;
    private const int MtdHoldBitPosition = 4;
    private const int KeyHoldBitPosition = 6;

    private string SettlementStatus { get; set; } = string.Empty;

    public RemarkSettingModel()
    {
    }

    public RemarkSettingModel(byte settlementStatus)
    {
        SettlementStatus = ConvertSettlementStatusToBinaryString(settlementStatus);
    }

    public bool IsPending => IsBitSet(PendingBitPosition);

    public bool IsUndecided => IsBitSet(UndecidedBitPosition);

    public bool IsUnderSettle => IsBitSet(UnderSettleBitPosition);

    public bool IsMtdHold => IsBitSet(MtdHoldBitPosition);

    public bool IsKeyHold => IsBitSet(KeyHoldBitPosition);

    private static string ConvertSettlementStatusToBinaryString(byte settlementStatus)
    {
        var binaryString = Convert.ToString(settlementStatus, BinaryBase);
        return new string([.. binaryString.Reverse()]);
    }

    private bool IsBitSet(int bitPosition)
    {
        return HasValidSettlementStatus() &&
               IsValidBitPosition(bitPosition) &&
               IsBitActive(bitPosition);
    }

    private bool HasValidSettlementStatus()
    {
        return !string.IsNullOrWhiteSpace(SettlementStatus);
    }

    private bool IsValidBitPosition(int bitPosition)
    {
        return SettlementStatus.Length > bitPosition;
    }

    private bool IsBitActive(int bitPosition)
    {
        return SettlementStatus[bitPosition].Equals(ActiveFlag);
    }
}