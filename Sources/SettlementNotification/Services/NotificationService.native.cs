namespace SettlementNotification.Services;

using Microsoft.Extensions.Logging;
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
    ILogger<NotificationService> logger)
    : NotificationService
{
    public void Notify(NotificationModel notificationModel)
    {
        try
        {
            logger.LogInformation($"Sending notification for MatchId: {notificationModel.MatchId}, MessageId: {notificationModel.MessageContent}");
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


            logger.LogError(logMessage, exception);

        }
        finally
        {
            notificationPoolService.Remove(notificationModel.MessageId);
        }
    }
}