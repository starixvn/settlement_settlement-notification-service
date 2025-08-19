using Fanex.Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Models;
using SettlementNotification.Models;
using System.Text.Json;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;

public interface IGetMatchTicketInfoHandler
{
    Task<List<TicketInfo>> HandleAsync(List<Match> matches);
}

public class GetMatchTicketInfoHandler(
    IDynamicRepository repository,
    IOptions<AppSettingsKey> settingOptions,
    ILogger<GetMatchTicketInfoHandler> logger) : IGetMatchTicketInfoHandler
{
    public async Task<List<TicketInfo>> HandleAsync(List<Match> matches)
    {
        if (matches.Count == 0)
        {
            return [];
        }


        var result = matches.SelectMany(match => match.ResourceIds.Select(resourceId =>
             new GetMatchTicketResource
             {
                 MatchId = match.MatchId,
                 ResourceId = resourceId,
                 SportType = match.SportType
             })).ToList();

        var json = JsonSerializer.Serialize(result);

        var criteria = new GetMatchTicketInfoCriteria
        {
            ResourceInfo = json
        };

        logger.LogInformation($"GetMatchTicketInfoHandler criteria ResourceInfo: {json}");

        var allTicketInfos = await GetAllTicketInfosFromAllSites(criteria);

        return AggregateTicketInfosByMatch(allTicketInfos);
    }


    private async Task<List<TicketInfo>> GetAllTicketInfosFromAllSites(GetMatchTicketInfoCriteria criteria)
    {
        var tasks = settingOptions.Value.SiteDbServer
            .Select(server => GetTicketInfosBySiteAsync(criteria, server))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return [.. results.SelectMany(ticketInfos => ticketInfos)];
    }


    private async Task<List<TicketInfo>> GetTicketInfosBySiteAsync(GetMatchTicketInfoCriteria criteria, string server)
    {
        criteria.DbServer = server;
        var ticketInfos = await repository.FetchAsync<TicketInfo>(criteria);

        return [.. ticketInfos];
    }

    private static List<TicketInfo> AggregateTicketInfosByMatch(List<TicketInfo> allTicketInfos)
    {
        return allTicketInfos
            .GroupBy(ticketInfo => ticketInfo.MatchId)
            .Select(x => new TicketInfo
            {
                MatchId = x.Key,
                IsExistsTicket = x.ToList()
                    .Exists(ticketInfo => ticketInfo.IsExistsTicket)
            })
            .ToList();
    }
}