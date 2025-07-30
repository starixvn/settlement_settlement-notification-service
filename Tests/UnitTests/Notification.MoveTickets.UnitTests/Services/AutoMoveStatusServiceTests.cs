using Fanex.Caching;
using Notification.MoveTickets.Constants;
using Notification.MoveTickets.Services;
using NSubstitute;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Services;

public class AutoMoveStatusServiceTests
{
    private readonly ICacheService cacheService;
    private readonly AutoMoveStatusService service;

    public AutoMoveStatusServiceTests()
    {
        cacheService = Substitute.For<ICacheService>();
        service = new AutoMoveStatusService(cacheService);
    }

    [Fact]
    public async Task GetAutoMoveStatus_KeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns(false);

        // Act
        var result = await service.GetAutoMoveStatus();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAutoMoveStatus_KeyExistsAndValueTrue_ReturnsTrue()
    {
        // Arrange
        cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns(true);
        cacheService.TryGetAsync<bool>(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns((true, true));

        // Act
        var result = await service.GetAutoMoveStatus();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetAutoMoveStatus_KeyExistsAndValueFalse_ReturnsFalse()
    {
        // Arrange
        cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns(true);
        cacheService.TryGetAsync<bool>(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns((true, false));

        // Act
        var result = await service.GetAutoMoveStatus();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAutoMoveStatus_KeyExistsAndTryGetFails_ReturnsFalse()
    {
        // Arrange
        cacheService.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns(true);
        cacheService.TryGetAsync<bool>(MoveTicketsMessages.CacheKeys.AutoMoveStatus).Returns((false, false));

        // Act
        var result = await service.GetAutoMoveStatus();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAutoMoveStatus_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        cacheService
            .When(x => x.ContainAsync(MoveTicketsMessages.CacheKeys.AutoMoveStatus))
            .Do(x => { throw new System.Exception(); });

        // Act
        var result = await service.GetAutoMoveStatus();

        // Assert
        Assert.False(result);
    }
}
