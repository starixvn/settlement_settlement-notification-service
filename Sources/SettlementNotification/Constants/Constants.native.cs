namespace SettlementNotification.Constants;

public static class MessageGroup
{
	public const int Instant = 1;

	public const int FastMarket = 2;

	public const int ConfirmRefund = 3;
}

public static class ChildMatchTypes
{
	public static readonly int[] Pen = [201, 202, 203];
}

public static class SportType
{
	public const short Soccer = 1;
}

public static class EventStatus
{
	public const string Completed = "completed";
	public const string Running = "running";
	public const string Postponed = "postponed";
	public const string RefundHt = "refund ht";
	public const string Refund = "refund";
	public const string Waiting = "Waiting";
	public const string Calculating = "Calculating";
	public const string Abandoned = "abandoned";
	public const string IrregularTime1H = "Irregular Time 1H";
	public const string IrregularTime = "Irregular Time";
	public const string WrongTeamName = "Wrong Team Name";
	public const string Suspended = "Suspended";
	public const string ResultPending = "Result Pending";
	public const string Internal = "internal";
	public const string Closed = "closed";
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