using Fanex.Data.Repository;
using Microsoft.Extensions.Options;
using Notification.MoveTickets.Features.GetMatchTicketInfo;
using NSubstitute;
using SettlementNotification.Models;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features
{
    public class GetMatchTicketInfoHandlerTests
    {
        private readonly IDynamicRepository repositorySub;
        private readonly IOptions<AppSettingsKey> options;
        private readonly GetMatchTicketInfoHandler handler;

        public GetMatchTicketInfoHandlerTests()
        {
            repositorySub = Substitute.For<IDynamicRepository>();
            var appSettings = new AppSettingsKey
            {
                SiteDbServer = new List<string> { "Server1", "Server2" }
            };
            options = Options.Create(appSettings);
            handler = new GetMatchTicketInfoHandler(repositorySub, options);
        }

        [Fact]
        public async Task HandleAsync_ReturnsAggregatedTickets()
        {
            // Arrange
            var matchIds = new List<int> { 1, 2 };
            var ticketsServer1 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsSettledTicket = true },
                new TicketInfo { MatchId = 2, IsExistsSettledTicket = false }
            };
            var ticketsServer2 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsSettledTicket = false },
                new TicketInfo { MatchId = 2, IsExistsSettledTicket = true }
            };
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server1"))
                .Returns(ticketsServer1);
            repositorySub.FetchAsync<TicketInfo>(Arg.Is<GetMatchTicketInfoCriteria>(c => c.DbServer == "Server2"))
                .Returns(ticketsServer2);

            // Act
            var result = await handler.HandleAsync(matchIds);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.MatchId == 1 && t.IsExistsSettledTicket);
            Assert.Contains(result, t => t.MatchId == 2 && t.IsExistsSettledTicket);
        }

        [Fact]
        public async Task HandleAsync_WithEmptyMatchIds_ReturnsEmptyList()
        {
            // Act
            var result = await handler.HandleAsync(new List<int>());

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
                new TicketInfo { MatchId = 1, IsExistsSettledTicket = false }
            };
            var ticketsServer2 = new List<TicketInfo>
            {
                new TicketInfo { MatchId = 1, IsExistsSettledTicket = true }
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
