using Notification.MoveTickets.Models;
using Notification.MoveTickets.Services;
using SettlementNotification.Constants;
using Xunit;

namespace Notification.MoveTickets.UnitTests.Services;

public class NotificationContentServiceTests
{
    private readonly NotificationContentService service;

    public NotificationContentServiceTests()
    {
        service = new NotificationContentService();
    }

    [Fact]
    public void CreateNotificationContent_WithAutoMoveEnabled_ShouldShowONStatus()
    {
        // Arrange
        var matches = CreateTestMatches();
        var isAutoMoveEnabled = true;

        // Act
        var result = service.CreateNotificationContent(matches, isAutoMoveEnabled);

        // Assert
        Assert.Contains("Auto move tickets of settled results service = ON.", result);
    }

    [Fact]
    public void CreateNotificationContent_WithAutoMoveDisabled_ShouldShowOFFStatus()
    {
        // Arrange
        var matches = CreateTestMatches();
        var isAutoMoveEnabled = false;

        // Act
        var result = service.CreateNotificationContent(matches, isAutoMoveEnabled);

        // Assert
        Assert.Contains("Auto move tickets of settled results service = OFF.", result);
    }

    [Fact]
    public void CreateNotificationContent_WithBaseballMatches_ShouldIncludeBaseballSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, ChildMatchId = 1 },
            new() { MatchId = 2, SportType = SportType.Baseball,ChildMatchId = 1},
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Baseball: 1, 2", result);
        Assert.DoesNotContain("+ Cricket: 3", result); // Cricket match should not be in eligible section
    }

    [Fact]
    public void CreateNotificationContent_WithCricketMatches_ShouldIncludeCricketSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Cricket,ChildMatchId = 1 },
            new() { MatchId = 2, SportType = SportType.Cricket,ChildMatchId = 1 },
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Cricket: 1, 2", result);
        Assert.DoesNotContain("+ Baseball: 3", result); // Baseball match should not be in eligible section
    }

    [Fact]
    public void CreateNotificationContent_WithOnHoldMatches_ShouldIncludeOnHoldSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 1 }, // Pending
            new() { MatchId = 2, SportType = SportType.Baseball, SettlementStatus = 2 }, // Undecided
            new() { MatchId = 3, SportType = SportType.Baseball, SettlementStatus = 4 }, // UnderSettle
            new() { MatchId = 4, SportType = SportType.Baseball, SettlementStatus = 16 }, // MtdHold
            new() { MatchId = 5, SportType = SportType.Baseball, SettlementStatus = 64 }  // KeyHold
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ matches on-hold: 1, 2, 3, 4, 5", result);
    }

    [Fact]
    public void CreateNotificationContent_WithMatchesWithoutChild_ShouldIncludeNoChildSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, ChildMatchId = null },
            new() { MatchId = 2, SportType = SportType.Baseball, ChildMatchId = 0 },
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ matches without corresponding child-match to move tickets: 1, 2", result);
    }



    [Fact]
    public void CreateNotificationContent_WithMixedScenarios_ShouldIncludeAllSections()
    {
        // Arrange
        var matches = new List<Match>
        {
            // Eligible matches
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 0, ChildMatchId = 100 },
            new() { MatchId = 2, SportType = SportType.Cricket, SettlementStatus = 0, ChildMatchId = 200 },

            // On-hold matches
            new() { MatchId = 3, SportType = SportType.Baseball, SettlementStatus = 1, ChildMatchId = 300 }, // Pending
            new() { MatchId = 4, SportType = SportType.Baseball, SettlementStatus = 2, ChildMatchId = 400 }, // Undecided

            // Matches without child
            new() { MatchId = 5, SportType = SportType.Baseball, SettlementStatus = 0, ChildMatchId = null },
            new() { MatchId = 6, SportType = SportType.Baseball, SettlementStatus = 0, ChildMatchId = 0 }
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        // Should include eligible matches
        Assert.Contains("+ Baseball: 1", result);
        Assert.Contains("+ Cricket: 2", result);

        // Should include on-hold matches
        Assert.Contains("+ matches on-hold: 3, 4", result);

        // Should include matches without child
        Assert.Contains("+ matches without corresponding child-match to move tickets: 5, 6", result);
    }

    [Fact]
    public void CreateNotificationContent_WithNoOnHoldMatches_ShouldNotIncludeOnHoldSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 0,ChildMatchId = 1 }, // Not on hold
            new() { MatchId = 2, SportType = SportType.Cricket, SettlementStatus = 0, ChildMatchId = 1 }  // Not on hold
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Baseball: 1", result);
        Assert.Contains("+ Cricket: 2", result);
        Assert.DoesNotContain("+ matches on-hold:", result);
    }

    [Fact]
    public void CreateNotificationContent_WithNoMatchesWithoutChild_ShouldNotIncludeNoChildSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, ChildMatchId = 100 }, // Has valid child
            new() { MatchId = 2, SportType = SportType.Cricket, ChildMatchId = 200 }  // Has valid child
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Baseball: 1", result);
        Assert.Contains("+ Cricket: 2", result);
        Assert.DoesNotContain("+ matches without corresponding child-match to move tickets:", result);
    }

    [Fact]
    public void CreateNotificationContent_WithNoBaseballMatches_ShouldNotIncludeBaseballSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Cricket, ChildMatchId = 1 },
            new() { MatchId = 2, SportType = SportType.Soccer,ChildMatchId = 1 }
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Cricket: 1", result);
        Assert.DoesNotContain("+ Baseball:", result);
    }

    [Fact]
    public void CreateNotificationContent_WithNoCricketMatches_ShouldNotIncludeCricketSection()
    {
        // Arrange
        var matches = new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball,ChildMatchId = 1 },
            new() { MatchId = 2, SportType = SportType.Soccer,ChildMatchId = 1 }
        };

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        Assert.Contains("+ Baseball: 1", result);
        Assert.DoesNotContain("+ Cricket:", result);
    }


    [Fact]
    public void CreateNotificationContent_ShouldHaveCorrectStructure()
    {
        // Arrange
        var matches = CreateTestMatches();

        // Act
        var result = service.CreateNotificationContent(matches, true);

        // Assert
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Should have service status
        Assert.Contains(lines, line => line.Contains("Auto move tickets of settled results service = ON."));

        // Should have eligible matches header
        Assert.Contains(lines, line => line.Contains("System detect following matches eligible for moving tickets of settled results:"));

        // Should have matches need resolve header
        Assert.Contains(lines, line => line.Contains("Matches need resolve:"));

        // Should have execution info
        Assert.Contains(lines, line => line.Contains("System shall execute move actions in the next 15 minutes"));
    }

    private static List<Match> CreateTestMatches()
    {
        return new List<Match>
        {
            new() { MatchId = 1, SportType = SportType.Baseball, SettlementStatus = 0, ChildMatchId = 100 },
            new() { MatchId = 2, SportType = SportType.Cricket, SettlementStatus = 0, ChildMatchId = 200 }
        };
    }
}