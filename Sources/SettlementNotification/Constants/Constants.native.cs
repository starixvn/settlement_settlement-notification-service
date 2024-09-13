namespace SettlementNotification.Constants;

public static class MessageGroup
{
    public const int Instant = 1;

    public const int FastMarket = 2;

    public const int ConfirmRefund = 3;
}

public static class SportType
{
    public const short Soccer = 1;
}

public static class SettlementStatus
{
    public const byte Running = 0;

    public const byte Refund = 1;

    public const byte RefundHt = 2;

    public const byte RefundFt = 3;

    public const byte Completed = 4;

    public const byte Abandoned1H = 5;

    public const byte Abandoned2H = 6;

    public const byte Abandoned = 7;

    public static byte[] RefundStatuses = [Refund, RefundHt, RefundFt, Abandoned1H, Abandoned2H, Abandoned];
}

public static class CacheKeys
{
    public const string KeyLeagueId = "Settlement-AutoPayout-AutoInstant-LeagueId";
}