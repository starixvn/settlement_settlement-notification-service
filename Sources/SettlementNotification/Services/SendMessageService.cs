namespace SettlementNotification.Services;

using Models;

public interface SendMessageService
{
    void Send(NotificationModel notificationModel);
}

public class SendMessageServiceImp(
    ContentService contentService,
    LetstalkClientService letstalkClientService,
    IOptions<AppSettingsKey> appSettingsKey)
    : SendMessageService
{
    public void Send(NotificationModel notificationModel)
    {
        var settings = appSettingsKey.Value;
        var content = contentService.Content(notificationModel);
        var letstalkGroupName = settings.EnvironmentName.Equals("PRO") && !notificationModel.IsTest
            ? settings.LetstalkUserGroup
            : settings.LetstalkTestGroup;

        var messageModel = CreateLetstalkMessageModel(letstalkGroupName, notificationModel.MessageGroupName, content);
        letstalkClientService.SendMessage(messageModel);
    }

    private LetstalkModel CreateLetstalkMessageModel(string letstalkGroupName, string subject, string content)
    {
        return new LetstalkModel
        {
            TargetName = appSettingsKey.Value.LetstalkTargetName,
            Subject = subject,
            Content = content,
            Group = letstalkGroupName
        };
    }
}