using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.MoveTickets.Jobs;
using Notification.MoveTickets.Models;
using Quartz;

namespace Notification.MoveTickets;

public static class QuartzConfiguration
{
    public static IServiceCollection AddQuartzServices(this IServiceCollection services, IConfiguration configuration)
    {
        var moveTicketsJobSettings = configuration.GetSection("MoveTicketsJobs").Get<MoveTicketsJobSettings>() ?? new();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("MatchReadyToMoveJob");
            q.AddJob<MatchReadyToMoveJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MatchReadyToMoveJob-trigger")
                .WithCronSchedule(moveTicketsJobSettings.MatchReadyToMoveJob.CronExpression)
            );


            //var moveTicketsJob = new JobKey("MoveTicketsJob");
            //q.AddJob<MoveTicketsJob>(opts => opts.WithIdentity(moveTicketsJob));

            //q.AddTrigger(opts => opts
            //    .ForJob(moveTicketsJob)
            //    .WithIdentity("MoveTicketsJob-trigger")
            //    .WithCronSchedule(moveTicketsJobSettings.MoveTicketsJob.CronExpression)
            //);
        });

        return services;
    }
}