using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Services;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using SettlementNotification.Models;

namespace Notification.MoveTickets.Features.MoveTicket;

public class MoveTicketHandler
{
    private readonly AppSettingsKey appSettingsKey;
    private readonly SiteDbClientService siteDbClientService;
    private readonly IDynamicRepository repository;

    public MoveTicketHandler(IOptions<AppSettingsKey> options, SiteDbClientService siteDbClientService, IDynamicRepository repository)
    {
        this.appSettingsKey = options.Value;
        this.siteDbClientService = siteDbClientService;
        this.repository = repository;
    }

    public async Task HandleAsync(MoveTicketCriteria criteria)
    {
        // get tickets that are ready to move
        var getTickets = new MoveTicketInfo();

        var sitesDb = await siteDbClientService.GetActiveSiteDbAsync();
        var tasks = sitesDb.Select(s => MoveTicket(s.SiteName, criteria))
                .ToArray();

        var result = await Task.WhenAll(tasks);

        await UpdateChildMatchInfo(getTickets.ChildMatchId);


    }

    private async Task<ResultMoveTicket> MoveTicket(string dbServer, MoveTicketCriteria criteria)
    {
        criteria.DbServer = dbServer;
        await repository.ExecuteAsync(criteria);

        return new ResultMoveTicket
        {
            ErrMsg = criteria.ErrMsg,
            ErrCode = criteria.ErrCode,
            TicketCount = criteria.TicketCount
        };
    }

    private async Task UpdateChildMatchInfo(int childMatchId)
    {
        var updateChildMatchCriteria = new UpdateChildMatchCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchCriteria);

        var updateChildMatchRemarkCriteria = new UpdateChildMatchRemarkCriteria { MatchId = childMatchId };
        await repository.ExecuteAsync(updateChildMatchRemarkCriteria);
    }
}
