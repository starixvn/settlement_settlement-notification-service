using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.Database;
using SettlementNotification.Models;
using System;

namespace SettlementNotification.Services;

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

    public NotificationServiceImp(
        RefDataService refDataService,
        QueryService queryService,
        SendMessageService sendMessageService,
        NotificationPoolService notificationPoolService)
    {
        this.refDataService = refDataService;
        this.queryService = queryService;
        this.sendMessageService = sendMessageService;
        this.notificationPoolService = notificationPoolService;
    }

    public void Notify(NotificationModel notificationModel)
    {
        try
        {
            notificationModel.RefDataService = refDataService;
            sendMessageService.Send(notificationModel);
            //queryService.UpdateFlag(notificationModel.MessageId);
            var logMessage1 = new LogMessage("Error from settlement notification service").ToString();
            LogService.Info(logMessage1);
        }
        catch (Exception exception)
        {
            var logMessage = new LogMessage($"Error from settlement notification service: {exception.Message}")
                .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(exception))
                .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(exception))
                .ToString();
            LogService.Info(logMessage);
        }
        finally
        {
            notificationPoolService.Remove(notificationModel.MessageId);
        }
    }
}