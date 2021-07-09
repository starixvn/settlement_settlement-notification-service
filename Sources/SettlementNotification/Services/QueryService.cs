using SettlementNotification.Criteria;
using SettlementNotification.Models;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;

namespace SettlementNotification.Services
{
    public interface QueryService
    {
        List<NotificationModel> GetAllMessage();

        void UpdateFlag(int id);
    }

    public class QueryServiceImp : QueryService
    {
        private readonly IDynamicRepository repository;

        public QueryServiceImp(IDynamicRepository repository)
        {
            this.repository = repository;
        }

        public List<NotificationModel> GetAllMessage()
        {
            var criteria = new GetAllMessageCriteria();
            return repository.Fetch<NotificationModel>(criteria).ToList();
        }

        public void UpdateFlag(int id)
        {
            var criteria = new UpdateFlagCriteria { MessageId = id };
            repository.Execute(criteria);
        }
    }
}