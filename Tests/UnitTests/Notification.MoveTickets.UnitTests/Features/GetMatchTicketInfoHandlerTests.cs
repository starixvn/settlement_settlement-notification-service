using Fanex.Data.Repository;
using Fanex.SiteDbConnectionManager.Client.Models;
using Fanex.SiteDbConnectionManager.Client.Services;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features
{
    public class GetMatchTicketInfoHandlerTests
    {
        private readonly IDynamicRepository repositorySub;
        private readonly GetMatchTicketInfoHandler handler;

        public GetMatchTicketInfoHandlerTests()
        {
            repositorySub = Substitute.For<IDynamicRepository>();
            var siteDbClientService = Substitute.For<SiteDbClientService>();
            siteDbClientService.GetActiveSiteDbAsync().Returns([
                new SiteDb { SiteName = "Server1", SiteDbGroupId = 1 },
                new SiteDb { SiteName = "Server2", SiteDbGroupId = 1 }
            ]);

            handler = new GetMatchTicketInfoHandler(repositorySub, siteDbClientService);
        }

        [Fact]
        public async Task HandleAsync_ReturnsAggregatedTickets()
        {
            // Arrange
            var matchIds = new List<int> { 1, 2 };
            var ticketsServer1 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsTicket = true },
                new TicketInfo { MatchId = 2, IsExistsTicket = false }
            };
            var ticketsServer2 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsTicket = false },
                new TicketInfo { MatchId = 2, IsExistsTicket = true }
            };
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server1"))
                .Returns(ticketsServer1);
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server2"))
                .Returns(ticketsServer2);

            // Act
            var result = await handler.HandleAsync(matchIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t is { MatchId: 1, IsExistsSettledTicket: true });
            Assert.Contains(result, t => t is { MatchId: 2, IsExistsSettledTicket: true });
        }

        [Fact]
        public async Task HandleAsync_WithEmptyMatchIds_ReturnsEmptyList()
        {
            // Act
            var result = await handler.HandleAsync([]);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task HandleAsync_GroupsTicketsByMatchId()
        {
            // Arrange
            var matchIds = new List<int> { 1 };
            var ticketsServer1 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsTicket = false }
            };
            var ticketsServer2 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsTicket = true }
            };
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server1"))
                .Returns(ticketsServer1);
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server2"))
                .Returns(ticketsServer2);

            // Act
            var result = await handler.HandleAsync(matchIds);

            // Assert
            Assert.Single(result);
            Assert.True(result.First().IsExistsSettledTicket);
        }
    }
}