# Notification.MoveTickets.Tests

This project contains comprehensive unit tests for the MoveTickets functionality in the Settlement Notification Service.

## Overview

The test project covers all aspects of the auto move tickets functionality including:

- **Jobs** - Testing the scheduled job execution logic
- **Features** - Testing business logic handlers
- **Models** - Testing data models and validation
- **Configuration** - Testing dependency injection and configuration

## Test Structure

### Jobs Tests

- **MatchReadyToMoveJobTests** - Tests for the job that detects eligible matches
- **MoveTicketsJobTests** - Tests for the job that executes ticket moving operations

### Features Tests

- **GetMatchesReadyToMoveHandlerTests** - Tests for the business logic handler

### Models Tests

- **MoveTicketModelsTests** - Tests for all data models and criteria classes

### Configuration Tests

- **MoveTicketsConfigurationTests** - Tests for dependency injection and configuration

## Test Coverage

### MatchReadyToMoveJob Tests (12 tests)

- ✅ No eligible matches scenario
- ✅ Eligible matches exist scenario
- ✅ Matches on hold detection
- ✅ Matches without child matches detection
- ✅ Exception handling
- ✅ Mixed match types processing
- ✅ Notification content generation
- ✅ Execution time inclusion
- ✅ All matches need resolution
- ✅ Only baseball matches
- ✅ Only cricket matches
- ✅ Content validation

### MoveTicketsJob Tests (13 tests)

- ✅ No eligible matches scenario
- ✅ Match on hold skipping
- ✅ Match without child skipping
- ✅ Valid match processing
- ✅ Multiple matches processing
- ✅ Exception handling
- ✅ Individual match failure handling
- ✅ Null move ticket info handling
- ✅ Mixed valid/invalid matches
- ✅ Move ticket operation exceptions
- ✅ All matches invalid scenario
- ✅ Statistics logging
- ✅ Edge case handling

### GetMatchesReadyToMoveHandler Tests (10 tests)

- ✅ No matches scenario
- ✅ Matches exist scenario
- ✅ Exception handling
- ✅ Correct criteria usage
- ✅ Multiple matches processing
- ✅ Different sport types
- ✅ Null repository response
- ✅ Empty enumerable response
- ✅ Null values handling
- ✅ Large dataset processing

### Models Tests (20 tests)

- ✅ MoveTicketInfo property setting
- ✅ ResultMoveTicket property setting
- ✅ Default values
- ✅ Match model properties
- ✅ MoveTicketCriteria validation
- ✅ GetMoveTicketInfoCriteria validation
- ✅ Stored procedure key generation
- ✅ Null value handling
- ✅ Edge case validation
- ✅ Configuration scenarios

### Configuration Tests (12 tests)

- ✅ Service registration
- ✅ Scoped lifetime verification
- ✅ Quartz configuration
- ✅ Valid cron expressions
- ✅ Invalid cron expressions
- ✅ Settings deserialization
- ✅ JobSchedule properties
- ✅ Null configuration handling
- ✅ Missing configuration
- ✅ Partial configuration
- ✅ Error scenarios
- ✅ Edge cases

## Running Tests

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code

### Running All Tests

```bash
dotnet test
```

### Running Specific Test Categories

```bash
# Run only job tests
dotnet test --filter "Category=Jobs"

# Run only model tests
dotnet test --filter "Category=Models"

# Run only configuration tests
dotnet test --filter "Category=Configuration"
```

### Running with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Dependencies

- **xUnit** - Testing framework
- **NSubstitute** - Mocking framework
- **Microsoft.NET.Test.Sdk** - Test SDK
- **coverlet.collector** - Code coverage

## Mocking Strategy

The tests use NSubstitute for mocking dependencies:

- **IDynamicRepository** - Database operations
- **ILogger<T>** - Logging
- **NotificationService** - Notification sending
- **MatchService** - Match-related operations
- **GetMatchesReadyToMoveHandler** - Business logic

## Test Data

Test data is created inline using object initializers to ensure tests are self-contained and readable.

## Best Practices

1. **AAA Pattern** - All tests follow Arrange, Act, Assert pattern
2. **Descriptive Names** - Test method names clearly describe the scenario
3. **Single Responsibility** - Each test focuses on one specific behavior
4. **Proper Assertions** - Use specific assertions rather than generic ones
5. **Mock Verification** - Verify that mocks are called with expected parameters
6. **Exception Testing** - Test both success and failure scenarios

## Continuous Integration

These tests are designed to run in CI/CD pipelines and provide fast feedback on code changes.

## Contributing

When adding new functionality to the MoveTickets feature:

1. Add corresponding tests for new classes
2. Update existing tests if behavior changes
3. Ensure all tests pass before submitting changes
4. Maintain test coverage above 80%

## Troubleshooting

### Common Issues

1. **Mock Setup Issues** - Ensure mocks are properly configured before use
2. **Async/Await** - Use proper async/await patterns in test methods
3. **Dependency Injection** - Verify services are properly registered in configuration tests

### Debugging Tests

Use the following command to run tests with detailed output:

```bash
dotnet test --verbosity normal
```

For even more detailed output:

```bash
dotnet test --verbosity detailed
```

## Test Statistics

- **Total Test Files**: 5
- **Total Test Methods**: 67+
- **Coverage Areas**: Jobs, Features, Models, Configuration
- **Mocking Framework**: NSubstitute
- **Testing Framework**: xUnit

## Architecture

The test project follows the same structure as the source code:

```
Tests/Notification.MoveTickets.Tests/
├── Jobs/                    # Job execution tests
├── Features/                # Business logic tests
├── Models/                  # Data model tests
├── Configuration/           # DI and config tests
└── README.md               # This file
```

## Performance Considerations

- Tests are designed to run quickly (< 1 second per test)
- Mocks are used to avoid external dependencies
- Test data is minimal and focused
- Async operations are properly tested

## Quality Gates

- All tests must pass before merging
- Code coverage should be > 80%
- No test should take longer than 1 second
- All async operations must be properly tested
- Exception scenarios must be covered
