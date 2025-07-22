using Fanex.Data;
using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;
public class GetMatchTicketInfoCriteria : CriteriaBase
{
    public string ListMatchIds { get; set; } = string.Empty;

    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Admin_Settlement_AutoMove_TicketInfo_{DbServer}";

    public override bool IsValid() => !string.IsNullOrEmpty(ListMatchIds);
}