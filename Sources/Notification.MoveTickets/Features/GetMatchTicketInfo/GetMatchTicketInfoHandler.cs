using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using SettlementNotification.Models;

namespace Notification.MoveTickets.Features.GetMatchTicketInfo;

public interface IGetMatchTicketInfoHandler
{
    Task<List<TicketInfo>> HandleAsync(List<int> matchIds);
}

public class GetMatchTicketInfoHandler(
    IDynamicRepository repository,
    IOptions<AppSettingsKey> options) : IGetMatchTicketInfoHandler
{
    private readonly AppSettingsKey appSettingsKey = options.Value;

    public async Task<List<TicketInfo>> HandleAsync(List<int> matchIds)
    {
        var criteria = new GetMatchTicketInfoCriteria()
        {
            ListMatchIds = string.Join(",", matchIds)
        };

        var tasks = appSettingsKey.SiteDbServer
                .Select(s => GetTicketInfosBySiteAsync(criteria, s))
                .ToArray();

        var result = await Task.WhenAll(tasks);

        var tickets = result.SelectMany(x => x)
            .GroupBy(x => x.MatchId)
            .Select(x => new TicketInfo
            {
                MatchId = x.Key,
                IsExistsSettledTicket = x.ToList().Exists(x => x.IsExistsSettledTicket)
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
