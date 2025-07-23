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
        
        services.AddQuartz(quartzConfigurator =>
        {
            ConfigureMatchReadyToMoveJob(quartzConfigurator, moveTicketsJobSettings.MatchReadyToMoveJob);
        });

        return services;
    }

    private static void ConfigureMatchReadyToMoveJob(IServiceCollectionQuartzConfigurator quartzConfigurator, JobSchedule jobSchedule)
    {
        var jobKey = new JobKey(nameof(MatchReadyToMoveJob));
        RegisterJobWithCronTrigger<MatchReadyToMoveJob>(quartzConfigurator, jobKey, jobSchedule.CronExpression);
    }

    private static void RegisterJobWithCronTrigger<T>(IServiceCollectionQuartzConfigurator quartzConfigurator, JobKey jobKey, string cronExpression) where T : IJob
    {
        quartzConfigurator.AddJob<T>(jobOptions => jobOptions.WithIdentity(jobKey));

        quartzConfigurator.AddTrigger(triggerOptions => triggerOptions
            .ForJob(jobKey)
            .WithIdentity($"{jobKey.Name}Trigger")
            .WithCronSchedule(cronExpression));
    }
}