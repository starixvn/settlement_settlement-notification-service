namespace Notification.MoveTickets.Features.GetMoveTicketInfo;

public class GetMoveTicketInfoHandler
{
    public async Task<List<MoveTicketInfo>> HandleAsync(GetMoveTicketInfoCriteria criteria)
    {
        // Simulate fetching move ticket info based on the criteria
        // In a real implementation, this would involve database calls or API requests
        if (!criteria.IsValid())
        {
            throw new ArgumentException("Invalid criteria for fetching move ticket info.");
        }
        // Example data, replace with actual data fetching logic
        var moveTicketInfos = new List<MoveTicketInfo>
        {
            new MoveTicketInfo
            {
                EventDate = DateTime.UtcNow,
                ChildMatchId = criteria.MatchId + 1,
                Resources = "Resource1, Resource2"
            }
        };
        return await Task.FromResult(moveTicketInfos);
    }
}
