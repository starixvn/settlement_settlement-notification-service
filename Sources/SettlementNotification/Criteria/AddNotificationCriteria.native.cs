namespace SettlementNotification.Criteria;

public class AddNotificationCriteria : NonQueryCommand
{
    public int MessageGroupId { get; set; } = 2;

    public string MessageType { get; } = "Reminder";

    public int MatchId { get; set; }

    public byte StatusId { get; set; }

    public string ActionName { get; set; }

    public string MessageContent { get; set; }

    public string MessageDetail { get; set; }

    public int UserId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; }
    public int MessageId { get; set; }

    public override bool IsValid() => MatchId > 0;

    public override string GetSettingKey() => "NAP_Set_LetstalkMessage_Insert";
}