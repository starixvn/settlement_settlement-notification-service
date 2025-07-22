namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public class Match
{
    public int MatchId { get; set; }
    public int SessionTime { get; set; }
    public int GameSession { get; set; }
    public DateTime? KickOffTime { get; set; }
    public short SportType { get; set; }
    public bool IsAutoMove { get; set; }
    public byte SettlementStatus { get; set; }
    public int? ChildMatchId { get; set; }
    public bool IsTest { get; set; }

    public RemarkSettingModel Remark => new(SettlementStatus);
}


public class RemarkSettingModel
{
    private const int ConvertToBase = 2;

    private const int UnderSettlePosition = 2;

    private const int MtdHoldPosition = 4;

    private const int KeyHoldPosition = 6;

    public string SettlementStatus { get; set; } = string.Empty;

    public RemarkSettingModel()
    {
    }

    public RemarkSettingModel(byte settlementStatus)
    {
        SettlementStatus = new string([.. Convert.ToString(settlementStatus, ConvertToBase).Reverse()]);
    }

    public bool IsPending => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus[0].Equals('1');

    public bool IsUndecided => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > 1 && SettlementStatus[1].Equals('1');

    public bool IsUnderSettle => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > UnderSettlePosition && SettlementStatus[UnderSettlePosition].Equals('1');

    public bool IsMtdHold => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > MtdHoldPosition && SettlementStatus[MtdHoldPosition].Equals('1');

    public bool IsKeyHold => !string.IsNullOrWhiteSpace(SettlementStatus) && SettlementStatus.Length > KeyHoldPosition && SettlementStatus[KeyHoldPosition].Equals('1');
}