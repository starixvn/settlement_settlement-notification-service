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
            // Job MatchReadyToMoveJob
            var readyJobKey = new JobKey(nameof(MatchReadyToMoveJob));
            q.AddJobAndTrigger<MatchReadyToMoveJob>(
                readyJobKey,
                moveTicketsJobSettings.MatchReadyToMoveJob.CronExpression);

            // Job MoveTicketsJob
            var moveJobKey = new JobKey(nameof(MoveTicketsJob));
            q.AddJobAndTrigger<MoveTicketsJob>(
                moveJobKey,
                moveTicketsJobSettings.MoveTicketsJob.CronExpression);
        });

        return services;
    }

    private static void AddJobAndTrigger<T>(this IServiceCollectionQuartzConfigurator quartz, JobKey jobKey, string cronExpression) where T : IJob
    {
        quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

        quartz.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity(jobKey.Name + "Trigger")
            .WithCronSchedule(cronExpression));
    }
}