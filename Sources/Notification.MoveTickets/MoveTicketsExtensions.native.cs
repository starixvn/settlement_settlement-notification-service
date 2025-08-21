using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using Notification.MoveTickets.Features.MoveTicket;
using Notification.MoveTickets.Features.UpdateMatch;
using Notification.MoveTickets.Features.VerifyBeforeMoveTickets;
using Notification.MoveTickets.Services;

namespace Notification.MoveTickets;

public static class MoveTicketsExtensions
{
    public static IServiceCollection AddMoveTickets(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddQuartzServices(configuration)
            .AddTransient<IGetMatchesReadyToMoveHandler, GetMatchesReadyToMoveHandler>()
            .AddTransient<IGetMatchTicketInfoHandler, GetMatchTicketInfoHandler>()
            .AddTransient<IGetMatchtInfoHandler, GetMatchtInfoHandler>()
            .AddTransient<IVerifyBeforeMoveTicketsHandler, VerifyBeforeMoveTicketsHandler>()
            .AddTransient<IMoveTicketHandler, MoveTicketHandler>()
            .AddTransient<IUpdateMatchHandler, UpdateMatchHandler>()
            .AddTransient<INotificationContentService, NotificationContentService>()
            .AddTransient<IAutoMoveStatusService, AutoMoveStatusService>()
            .AddTransient<IMatchesReadyToMoveService, MatchesReadyToMoveService>()
            .AddTransient<IMoveTicketService, MoveTicketService>();

        return services;
    }
}