using Fanex.Data;
using Fanex.Data.Repository;
using System.Text.Json.Serialization;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;
public class GetMatchTicketInfoCriteria : CriteriaBase
{
    public string ResourceInfo { get; set; } = string.Empty;

    [SpParam(Ignored = true)]
    public string DbServer { get; set; } = string.Empty;

    public override string GetSettingKey() => $"NAP_Admin_Settlement_AutoMove_TicketInfo_{DbServer}";

    public override bool IsValid() => !string.IsNullOrEmpty(ResourceInfo);
}

public class GetMatchTicketResource
{
    [JsonPropertyName("m")]
    public int MatchId { get; set; }

    [JsonPropertyName("r")]
    public string ResourceId { get; set; }

    [JsonPropertyName("s")]

    public int SportType { get; set; }
}