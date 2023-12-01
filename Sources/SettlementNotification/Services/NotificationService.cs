namespace SettlementNotification.Services;

using Models;

public interface NotificationService
{
    void Notify(NotificationModel notificationModel);
}

public class NotificationServiceImp : NotificationService
{
    private readonly RefDataService refDataService;

    private readonly QueryService queryService;

    private readonly SendMessageService sendMessageService;

    private readonly NotificationPoolService notificationPoolService;

    private readonly ILogger logger;

    public NotificationServiceImp(
        RefDataService refDataService,
        QueryService queryService,
        SendMessageService sendMessageService,
        NotificationPoolService notificationPoolService,
        ILogger logger)
    {
        this.refDataService = refDataService;
        this.queryService = queryService;
        this.sendMessageService = sendMessageService;
        this.notificationPoolService = notificationPoolService;
        this.logger = logger;
    }

    public void Notify(NotificationModel notificationModel)
    {
        try
        {
            notificationModel.RefDataService = refDataService;
            sendMessageService.Send(notificationModel);
            queryService.UpdateFlag(notificationModel.MessageId);
        }
        catch (Exception exception)
        {
            var logMessage = new LogMessage($"Error from settlement notification service: {exception.Message}")
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            logger.Error(logMessage);
        }
        finally
        {
            notificationPoolService.Remove(notificationModel.MessageId);
        }
    }
}