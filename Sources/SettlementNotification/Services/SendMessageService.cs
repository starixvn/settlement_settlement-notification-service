using SettlementNotification.Models;
using Fanex.LetstalkService.Client;
using Fanex.LetstalkService.Core.Models;
using Microsoft.Extensions.Options;

namespace SettlementNotification.Services
{
    public interface SendMessageService
    {
        void Send(string subject, string message);
    }

    public class SendMessageServiceImp : SendMessageService
    {
        private readonly AppSettingsKey appSettingskey;

        private readonly LetstalkClientService letstalkClientService;

        public SendMessageServiceImp(LetstalkClientService letstalkClientService, IOptions<AppSettingsKey> appSettingskey)
        {
            this.letstalkClientService = letstalkClientService;
            this.appSettingskey = appSettingskey.Value;
        }

        public void Send(string subject, string message)
        {
            var messageModel = CreateLetstalkMessageModel(subject, message);
            letstalkClientService.SendMessage(messageModel);
        }

        private LetstalkModel CreateLetstalkMessageModel(string subject, string message)
        {
            return new LetstalkModel
            {
                TargetName = appSettingskey.LetstalkTargetName,
                Subject = subject,
                Content = message,
                Group = appSettingskey.LetstalkGroupName
            };
        }
    }
}