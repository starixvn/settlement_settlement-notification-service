# Settlement Notification Service - Auto Move Tickets

This service implements an automated ticket moving system for settled results in sports betting, specifically designed for Baseball and Cricket matches.

## Overview

The auto move tickets functionality consists of two main jobs that work together:

1. **MatchReadyToMoveJob** - Detects eligible matches and sends notifications
2. **MoveTicketsJob** - Executes the actual ticket moving operations

## Features

### MatchReadyToMoveJob

- Detects matches eligible for moving tickets of settled results
- Separates matches by sport type (Baseball and Cricket)
- Identifies matches that need resolution:
  - Matches on hold (`@match_onhold`)
  - Matches without corresponding child matches (`@match_nochild`)
- Sends notifications with detailed information about eligible matches
- Executes at 11:30 AM daily (configurable via cron expression)

### MoveTicketsJob

- Executes ticket moving operations for eligible matches
- Validates match conditions before moving tickets:
  - Checks if match is on hold
  - Verifies child match exists
- Skips unresolved matches as per requirements
- Provides detailed logging of success/failure/skip counts
- Executes at 11:45 AM daily (15 minutes after detection job)

## Configuration

### Job Scheduling

Jobs are configured in `appsettings.json` files:

```json
{
  "MoveTicketsJobs": {
    "MatchReadyToMoveJob": {
      "CronExpression": "0 30 11 * * ?",
      "Description": "Get matches ready to move at 11:30 AM daily"
    },
    "MoveTicketsJob": {
      "CronExpression": "0 45 11 * * ?",
      "Description": "Move tickets at 11:45 AM daily"
    }
  }
}
```

### Sport Types

Supported sport types are defined in `Constants.cs`:

- Soccer = 1
- Baseball = 2
- Cricket = 3

## Database Stored Procedures

The system relies on the following stored procedures:

1. **NAP_Admin_Settlement_GetMatchesReadyToMove** - Retrieves eligible matches
2. **NAP_Admin_Settlement_50_MoveTicketInfo** - Gets move ticket information for a match
3. **NAP*Admin_Settlement_50_MoveTicket*{DbServer}** - Executes ticket moving operation

## Notification Content Format

The system generates notifications with the following format:

```
Auto move tickets of settled results service = @move_setting.

System detect following matches eligible for moving tickets of settled results:
+ Baseball: @baseball_match (X matches)
+ Cricket: @cricket_match_to (Y matches)

Matches need resolve:
+ matches on-hold: @match_onhold (Z matches)
+ matches without corresponding child-match to move tickets: @match_nochild (W matches)

System shall execute move actions in the next 15 minutes (23:45 GMT-4).
Unresolved matches will not be moved.
System shall also move ticket on new match found that eligible to move tickets.
```

## Architecture

### Key Components

1. **MatchReadyToMoveJob** - Main detection job
2. **MoveTicketsJob** - Main execution job
3. **GetMatchesReadyToMoveHandler** - Business logic for match detection
4. **MatchService** - Service for match-related operations
5. **NotificationService** - Service for sending notifications

### Models

- **Match** - Represents a match with sport type information
- **MoveTicketInfo** - Contains move ticket operation details
- **ResultMoveTicket** - Contains operation result information
- **MoveTicketCriteria** - Criteria for move ticket operations

## Error Handling

The system includes comprehensive error handling:

- Individual match processing errors don't stop the entire job
- Detailed logging for debugging and monitoring
- Graceful handling of missing data or invalid conditions
- Retry mechanisms through Quartz job scheduling

## Logging

The system provides detailed logging at various levels:

- **Information** - Job start/completion, match counts, successful operations
- **Warning** - Skipped matches (on hold, no child match)
- **Error** - Failed operations, exceptions

## Dependencies

- **Fanex.Data.Repository** - Database operations
- **Quartz.AspNetCore** - Job scheduling
- **Microsoft.Extensions.Logging** - Logging framework
- **SettlementNotification** - Core notification functionality

## Usage

The service runs automatically based on the configured cron expressions. No manual intervention is required for normal operation.

### Manual Testing

For testing purposes, you can modify the cron expressions in the configuration files:

```json
"MoveTicketsJobs": {
  "MatchReadyToMoveJob": {
    "CronExpression": "*/5 * * * * ?",  // Every 5 seconds
    "Description": "Test mode - every 5 seconds"
  },
  "MoveTicketsJob": {
    "CronExpression": "*/10 * * * * ?", // Every 10 seconds
    "Description": "Test mode - every 10 seconds"
  }
}
```

## Security

- System operations use a dedicated admin ID (1) for audit purposes
- All operations are logged with detailed information
- No sensitive data is exposed in logs
- Database operations use parameterized queries

## Monitoring

Monitor the service through:

1. **Application logs** - Check for job execution and error messages
2. **Database** - Verify stored procedure execution and results
3. **Notification system** - Confirm notifications are being sent
4. **Health checks** - Use the `/healthz` endpoint for service health
