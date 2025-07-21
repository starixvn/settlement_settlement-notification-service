using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.MoveTickets.Jobs;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using SettlementNotification.Services;

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
            .AddScoped<GetMatchesReadyToMoveHandler>();

        return services;
    }
}
