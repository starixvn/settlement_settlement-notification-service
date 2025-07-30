using Fanex.Data.Repository;
using Notification.MoveTickets.Features.GetMoveTicketInfo;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features
{
    public class GetMatchtInfoHandlerTests
    {
        private readonly IDynamicRepository repositorySub;
        private readonly GetMatchtInfoHandler handler;

        public GetMatchtInfoHandlerTests()
        {
            repositorySub = Substitute.For<IDynamicRepository>();
            handler = new GetMatchtInfoHandler(repositorySub);
        }

        [Fact]
        public async Task HandleAsync_ReturnsMatchMoveInfo()
        {
            // Arrange
            var matchId = 123;
            var expected = new MatchMoveInfo { ChildMatchId = 456, EventDate = DateTime.UtcNow, Resources = "res" };
            repositorySub.GetAsync<MatchMoveInfo>(Arg.Any<GetMatchtInfoCriteria>()).Returns(expected);

            // Act
            var result = await handler.HandleAsync(matchId);

            // Assert
            Assert.Equal(expected.ChildMatchId, result.ChildMatchId);
            Assert.Equal(expected.EventDate, result.EventDate);
            Assert.Equal(expected.Resources, result.Resources);
        }

        [Fact]
        public async Task HandleAsync_RepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            repositorySub.GetAsync<MatchMoveInfo>(Arg.Any<GetMatchtInfoCriteria>()).Returns((MatchMoveInfo)null);

            // Act
            var result = await handler.HandleAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
