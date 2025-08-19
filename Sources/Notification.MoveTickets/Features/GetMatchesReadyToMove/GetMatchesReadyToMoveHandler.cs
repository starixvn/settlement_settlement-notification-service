using Fanex.Caching;
using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Models;
using SettlementNotification.Models;

namespace Notification.MoveTickets.Features.GetMatchesReadyToMove;

public interface IGetMatchesReadyToMoveHandler
{
    Task<List<Match>> HandleAsync();
}

public class GetMatchesReadyToMoveHandler(
    IDynamicRepository repository,
    IOptions<AppSettingsKey> appSettingsKey,
    ICache cache,
    ILogger<GetMatchesReadyToMoveHandler> logger
 ) : IGetMatchesReadyToMoveHandler
{
    private const string KEY_LEAGUEIDS_TEST = "Settlement-AutoSettle-LeagueIds";

    public async Task<List<Match>> HandleAsync()
    {
        var criteria = new GetMatchesReadyToMoveCriteria();
        var matches = await repository.FetchAsync<Match>(criteria);



        var (_, response) = await cache.TryGetAsync<string>(KEY_LEAGUEIDS_TEST);
        var leagueIds = (response ?? "").Split(",");
        logger.LogInformation($"GetMatchesReadyToMoveHandler found {response} {string.Join(", ", matches.Select(x => x.ToString()))} matches ready to move.");


        var newMatches = matches
            .GroupBy(m => m.MatchId)
            .Select(g => new Match
            {
                MatchId = g.Key,
                LeagueId = g.First().LeagueId,
                SportType = g.First().SportType,
                ResourceId = g.First().ResourceId,
                ResourceIds = [.. g.Select(x => x.ResourceId)],
                SessionTime = g.First().SessionTime,
                GameSession = g.First().GameSession,
                KickOffTime = g.First().KickOffTime,
                ChildMatchId = g.First().ChildMatchId,
                SettlementStatus = g.First().SettlementStatus,
                IsTest = g.First().IsTest,
            }).ToList();

        var filteredMatches = appSettingsKey.Value.EnvironmentName switch
        {
            "PRO" => leagueIds.Length > 0
                ? [.. newMatches.Where(m => !leagueIds.Contains(m.LeagueId.ToString()))]
                : newMatches,
            "UAT" => [.. newMatches.Where(m => leagueIds.Contains(m.LeagueId.ToString()))],
            _ => newMatches
        };

        logger.LogInformation($"GetMatchesReadyToMoveHandler found {string.Join(", ", matches.Select(x => x.ToString()))} matches ready to move.");


        return [.. filteredMatches];
    }
}