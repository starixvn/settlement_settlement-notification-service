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
        var criteria = new GetMatchTicketInfoCriteria()
        {
            ListMatchIds = string.Join(",", matchIds)
        };
        var sitesDb = await siteDbClientService.GetActiveSiteDbAsync();
        var tasks = sitesDb.Select(s => GetTicketInfosBySiteAsync(criteria, s.SiteName))
                .ToArray();

        var result = await Task.WhenAll(tasks);

        var tickets = result.SelectMany(x => x)
            .GroupBy(x => x.MatchId)
            .Select(x => new TicketInfo
            {
                MatchId = x.Key,
                IsExistsSettledTicket = x.ToList().Exists(ticketInfo => ticketInfo.IsExistsSettledTicket)
            })
            .ToList();

        return tickets;
    }

    private async Task<List<TicketInfo>> GetTicketInfosBySiteAsync(GetMatchTicketInfoCriteria criteria, string server)
    {
        criteria.DbServer = server;

        return (await repository.FetchAsync<TicketInfo>(criteria)).ToList();
    }
}