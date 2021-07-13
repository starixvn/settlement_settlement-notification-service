using SettlementNotification.Criteria;
using SettlementNotification.Models;
using Fanex.Data.Repository;
using System.Collections.Generic;
using System.Linq;
using Fanex.Caching;
using Microsoft.Extensions.Options;

namespace SettlementNotification.Services
{
    public interface QueryService
    {
        List<NotificationModel> GetAllMessage();

        void UpdateFlag(int id);
    }

    public class QueryServiceImp : QueryService
    {
        private const string KEY_LEAGUEID = "Settlement-AutoPayout-AutoInstant-LeagueId";

        private readonly ICache cache;

        private readonly AppSettingsKey appSettingsKey;

        private readonly IDynamicRepository repository;

        public QueryServiceImp(IDynamicRepository repository, ICache cache, IOptions<AppSettingsKey> appSettingsKey)
        {
            this.repository = repository;
            this.cache = cache;
            this.appSettingsKey = appSettingsKey.Value;
        }

        public List<NotificationModel> GetAllMessage()
        {
            var criteria = new GetAllMessageCriteria();
            var matches = repository.Fetch<NotificationModel>(criteria).ToList();

            cache.TryGet(KEY_LEAGUEID, out int leagueId);

            if (appSettingsKey.EnvironmentName.Equals("PRO"))
            {
                return leagueId > 0
                    ? (from m in matches where m.LeagueId != leagueId select m).ToList()
                    : matches;
            }
            else
            {
                return (from m in matches where m.LeagueId == leagueId select m).ToList();
            }
        }

        public void UpdateFlag(int id)
        {
            var criteria = new UpdateFlagCriteria { MessageId = id };
            repository.Execute(criteria);
        }
    }
}