namespace SettlementNotification.Models;

public class AppSettingsKey
{
    public LetstalkSettings Letstalk { get; set; }

    public string EnvironmentName { get; set; }

    public List<string> SiteDbServer { get; set; }
}