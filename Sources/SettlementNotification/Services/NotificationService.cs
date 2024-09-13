namespace SettlementNotification.Services;

using Models;

public interface NotificationService
{
    void Notify(NotificationModel notificationModel);
}

public class NotificationServiceImp(
    RefDataService refDataService,
    QueryService queryService,
    SendMessageService sendMessageService,
    NotificationPoolService notificationPoolService,
    ILogger logger)
    : NotificationService
{
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
            var logMessage = new LogMessage($"MatchId: {notificationModel.MatchId} - MessageId: {notificationModel.MessageId} : {exception.Message}")
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