using SettlementNotification.Constants;

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
        var content = contentService.Content(notificationModel);
        var letstalkGroupName = GetLetstalkGroup(notificationModel.LetstalkGroup, notificationModel.IsTest);

        var messageModel = CreateLetstalkMessageModel(letstalkGroupName, notificationModel.MessageGroupName, content);
        letstalkClientService.SendMessage(messageModel);
    }

    private string GetLetstalkGroup(int groupId, bool isTest)
    {
        var settings = appSettingsKey.Value;
        var isProdEnvironment = settings.EnvironmentName.Equals("PRO") && !isTest;

        if (groupId == LetstalkGroup.RefundReminder)
        {
            return isProdEnvironment ? settings.Letstalk.UserGroupRefund : settings.Letstalk.TestGroup;
        }

        return isProdEnvironment ? settings.Letstalk.UserGroupNotification : settings.Letstalk.TestGroup;
    }

    private LetstalkModel CreateLetstalkMessageModel(string letstalkGroupName, string subject, string content)
    {
        return new LetstalkModel
        {
            TargetName = appSettingsKey.Value.Letstalk.TargetName,
            Subject = subject,
            Content = content,
            Group = letstalkGroupName
        };
    }
}