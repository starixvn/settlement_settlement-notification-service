using SettlementNotification.Constants;

namespace SettlementNotification.Services;

using Fanex.LetstalkService.Client.Models;
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
        var (letstalkGroupName, environmentName) = GetLetstalkGroup(notificationModel.IsTest, notificationModel.MessageGroupId);
        var subject = $"{environmentName}{notificationModel.MessageGroupName}";
        var messageModel = CreateLetstalkMessageModel(letstalkGroupName, subject, content);
        letstalkClientService.SendMessage(messageModel);
    }

    private (string groupName, string environmentName) GetLetstalkGroup(bool isTest, int messageGroupId)
    {
        var settings = appSettingsKey.Value;
        var isProdEnvironment = settings.EnvironmentName.Equals("PRO") && !isTest;
        var environmentName = isProdEnvironment ? string.Empty : $"{settings.EnvironmentName} - ";

        var groupName = GetNotificationGroupName(isProdEnvironment, messageGroupId);

        return (groupName, environmentName);
    }

    private string GetNotificationGroupName(bool isProdEnvironment, int messageGroupId)
    {
        var settings = appSettingsKey.Value;

        var prodGroupName = messageGroupId switch
        {
            MessageGroup.HybridParlay => settings.Letstalk.UserGroupHybridParlay,
            MessageGroup.ConfirmRefund => settings.Letstalk.UserGroupRefund,
            MessageGroup.AutoRefundFastMarket => settings.Letstalk.UserGroupRefundFastMarket,
            _ => settings.Letstalk.UserGroupNotification
        };

        return isProdEnvironment ? prodGroupName : settings.Letstalk.TestGroup;
    }

    private LetstalkModel CreateLetstalkMessageModel(string letstalkGroupName, string subject, string content)
        => new LetstalkModel
        {
            TargetName = appSettingsKey.Value.Letstalk.TargetName,
            Subject = subject,
            Content = content,
            Group = letstalkGroupName
        };
}