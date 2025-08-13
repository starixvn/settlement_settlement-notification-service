using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Services;
using Notification.MoveTickets.Models;
using System.Text.Json;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;

public interface IGetMatchTicketInfoHandler
{
    Task<List<TicketInfo>> HandleAsync(List<Match> matches);
}

public class GetMatchTicketInfoHandler(
    IDynamicRepository repository,
    SiteDbClientService siteDbClientService) : IGetMatchTicketInfoHandler
{
    public async Task<List<TicketInfo>> HandleAsync(List<Match> matches)
    {
        var resources = matches.Select(x => new GetMatchTicketResource
        {
            MatchId = x.MatchId,
            SportType = x.SportType,
            ResourceId = x.ResourceId
        });

        var json = JsonSerializer.Serialize(resources, new JsonSerializerOptions
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
        var activeSites = await siteDbClientService.GetActiveSiteDbAsync();
        var ticketInfoTasks = CreateTicketInfoTasks(criteria, activeSites);

        var results = await Task.WhenAll(ticketInfoTasks);

        return [.. results.SelectMany(ticketInfos => ticketInfos)];
    }

    private Task<List<TicketInfo>>[] CreateTicketInfoTasks(GetMatchTicketInfoCriteria criteria, IEnumerable<dynamic> activeSites)
    {
        return [.. activeSites
            .Select(site => GetTicketInfosBySiteAsync(criteria, site.SiteName))
            .Cast<Task<List<TicketInfo>>>()];
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