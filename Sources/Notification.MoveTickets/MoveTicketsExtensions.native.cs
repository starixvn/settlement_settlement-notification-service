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
            .AddScoped<IGetMatchesReadyToMoveHandler, GetMatchesReadyToMoveHandler>()
            .AddScoped<IGetMatchTicketInfoHandler, GetMatchTicketInfoHandler>()
            .AddScoped<IGetMatchtInfoHandler, GetMatchtInfoHandler>()
            .AddScoped<IVerifyBeforeMoveTicketsHandler, VerifyBeforeMoveTicketsHandler>()
            .AddScoped<IMoveTicketHandler, MoveTicketHandler>()
            .AddScoped<IUpdateMatchHandler, UpdateMatchHandler>()
            .AddScoped<INotificationContentService, NotificationContentService>()
            .AddScoped<IAutoMoveStatusService, AutoMoveStatusService>()
            .AddScoped<IMatchesReadyToMoveService, MatchesReadyToMoveService>()
            .AddScoped<IMoveTicketService, MoveTicketService>();

        return services;
    }
}