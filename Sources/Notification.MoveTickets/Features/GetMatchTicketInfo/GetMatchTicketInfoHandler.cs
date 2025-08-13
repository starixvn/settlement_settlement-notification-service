using Fanex.Data.Repository;
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
    IOptions<AppSettingsKey> settingOptions) : IGetMatchTicketInfoHandler
{
    public async Task<List<TicketInfo>> HandleAsync(List<Match> matches)
    {
        var result = matches.SelectMany(match => match.ResourceIds.Select(resourceId =>
             new GetMatchTicketResource
             {
                 MatchId = match.MatchId,
                 ResourceId = resourceId,
                 SportType = match.SportType
             })).ToList();

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var criteria = new GetMatchTicketInfoCriteria
        {
            ResourceInfo = json
        };

        var allTicketInfos = await GetAllTicketInfosFromAllSites(criteria);

        return AggregateTicketInfosByMatch(allTicketInfos);
    }


    private async Task<List<TicketInfo>> GetAllTicketInfosFromAllSites(GetMatchTicketInfoCriteria criteria)
    {
        var tasks = settingOptions.Value.SiteDbServer
            .Select(server => GetTicketInfosBySiteAsync(criteria, server))
            .ToList();

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