using Fanex.Data.Repository;
using Notification.MoveTickets.Features.GetMatchesReadyToMove;
using Notification.MoveTickets.Models;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Features
{
    public class GetMatchesReadyToMoveHandlerTests
    {
        private readonly IDynamicRepository repositorySub;
        private readonly GetMatchesReadyToMoveHandler handler;

        public GetMatchesReadyToMoveHandlerTests()
        {
            repositorySub = Substitute.For<IDynamicRepository>();
            handler = new GetMatchesReadyToMoveHandler(repositorySub);
        }

        [Fact]
        public async Task HandleAsync_ReturnsMatchesFromRepository()
        {
            // Arrange
            var expectedMatches = new List<Match>
            {
                new Match { MatchId = 1 },
                new Match { MatchId = 2 }
            };
            repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
                .Returns(expectedMatches);

            // Act
            var result = await handler.HandleAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].MatchId);
            Assert.Equal(2, result[1].MatchId);
        }

        [Fact]
        public async Task HandleAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            repositorySub.FetchAsync<Match>(Arg.Any<GetMatchesReadyToMoveCriteria>())
                .Returns(new List<Match>());

            // Act
            var result = await handler.HandleAsync();

            // Assert
            Assert.Empty(result);
        }
    }
}
