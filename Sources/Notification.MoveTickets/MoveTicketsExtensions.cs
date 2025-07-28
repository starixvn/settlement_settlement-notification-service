using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using Notification.MoveTickets.Services;

namespace Notification.MoveTickets;

public static class MoveTicketsExtensions
{
    public static IServiceCollection AddMoveTickets(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddQuartzServices(configuration)
            .AddScoped<MatchReadyToMoveJob>()
            .AddScoped<MoveTicketsJob>()
            .AddScoped<MatchService, MatchServiceImp>()
            .AddScoped<IGetMatchesReadyToMoveHandler, GetMatchesReadyToMoveHandler>()
            .AddScoped<IGetMatchTicketInfoHandler, GetMatchTicketInfoHandler>()
            .AddScoped<INotificationContentService, NotificationContentService>();

        return services;
    }
}