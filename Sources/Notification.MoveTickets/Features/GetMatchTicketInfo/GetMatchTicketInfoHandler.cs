using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Services;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;

public interface IGetMatchTicketInfoHandler
{
    Task<List<TicketInfo>> HandleAsync(List<int> matchIds);
}

public class GetMatchTicketInfoHandler(
    IDynamicRepository repository,
    SiteDbClientService siteDbClientService) : IGetMatchTicketInfoHandler
{
    public async Task<List<TicketInfo>> HandleAsync(List<int> matchIds)
    {
        var criteria = new GetMatchTicketInfoCriteria
        {
            ListMatchIds = string.Join(",", matchIds)
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
                IsExistsSettledTicket = x.ToList()
                    .Exists(ticketInfo => ticketInfo.IsExistsSettledTicket)
            })
            .ToList();
    }
}