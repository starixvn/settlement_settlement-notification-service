namespace SettlementNotification.Constants;

public static class MessageGroup
{
    public const int Instant = 1;

    public const int FastMarket = 2;

    public const int ConfirmRefund = 3;
}

public static class LetstalkGroup
{
    public const int Notification = 1;

    public const int RefundReminder = 2;
}

public static class SportType
{
    public const short Soccer = 1;
}

public static class SettlementStatus
{
    private const byte Refund = 1;

    private const byte RefundHt = 2;

    private const byte RefundFt = 3;

    private const byte Abandoned1H = 5;

    private const byte Abandoned2H = 6;

    private const byte Abandoned = 7;

    public static readonly byte[] RefundStatuses = [Refund, RefundHt, RefundFt, Abandoned1H, Abandoned2H, Abandoned];
}

public static class CacheKeys
{
    public const string KeyLeagueId = "Settlement-AutoPayout-AutoInstant-LeagueId";
}