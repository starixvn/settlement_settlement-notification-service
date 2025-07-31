using Fanex.Data.Repository;

namespace Notification.MoveTickets.Features.UpdateMatch;
public class UpdateChildMatchCriteria : NonQueryCommand
{
    public int MatchId { get; set; }

    public string Command => "NAP_Admin_Settlement_1-CAL";

    public int FtHome { get; set; }

    public int FtAway { get; set; }

    public int HtHome { get; set; }

    public int HtAway { get; set; }

    public string Action => "CAL";

    public bool IsMovedMatch => true;

    public override string GetSettingKey()
    {
        return "NAP_Admin_Match_Update";
    }

    public override bool IsValid()
    {
        return MatchId > 0;
    }
}