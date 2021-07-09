using Fanex.RemotingRef;

namespace SettlementNotification.Services
{
    public interface RefDataService
    {
        string GetTeamName(object teamId);

        string GetLeagueName(object leagueId);
    }

    public class RefDataServiceImp : RefDataService
    {
        private const string LanguageCode = "en";

        private readonly IRemotingRefService remotingRefService;

        public RefDataServiceImp(IRemotingRefService remotingRefService)
        {
            this.remotingRefService = remotingRefService;
        }

        public string GetTeamName(object teamId)
        {
            return remotingRefService.GetTeamName(teamId.ToString(), LanguageCode, string.Empty);
        }

        public string GetLeagueName(object leagueId)
        {
            return remotingRefService.GetLeagueName(leagueId.ToString(), LanguageCode, string.Empty);
        }
    }
}