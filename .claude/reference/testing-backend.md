# Backend Testing Guidelines

## Overview

Backend testing uses xUnit with XML documentation comments in Gherkin style:
- **Unit Tests** - Domain logic, handlers, specifications
- **Integration Tests** - Authorization policy testing only

## Unit Tests

### Location
```
tests/
```

### Framework
- **Asm.Testing.Domain** - Extensions for mocking DbSet
- **Microsoft.Testing.Platform** - Test host and utilities (as opposed to VSTest)
- **xUnit** - Underlying test runner (xUnit.v3)
- **Moq** - Mocking

### What to Test
- Domain entity business logic and invariants
- Specification logic
- Command/Query handlers (with mocked repositories and IQueryables)
- Complex calculations (e.g., balance computations, streak calculations)

### Test File Structure

Example of a module test structure:
```
tests/Asm.MooBank.Modules.Accounts.Tests/
├── Commands/
│   ├── CreateTests.cs
│   └── UpdateTests.cs
└── Queries/
    ├── GetTests.cs
    └── GetAllTests.cs
```

### Example Test

Tests MUST be properly documented with Gherkin-style comments.

```csharp
/// <summary>
/// Unit tests for the <see cref="Calculator"/> class.
/// </summary>
public class CalculatorTests
{
    /// <summary>
    /// Given two numbers
    /// When the Add method is called
    /// Then the result should be their sum
    /// </summary>
    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-1, 1, 0)]
    [Trait("Category", "Unit")]
    public void ShouldAddTwoNumbersCorrectly(int left, int right, int expected)
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(left, right);
        
        // Assert
        Assert.Equal(expected, result);
    }
}
```

## API Integration Tests (Authorization Only)

### Purpose
API integration tests are **specifically for testing authorization policies**. They verify that:
- Authenticated users can access their own resources
- Users cannot access other users' resources
- Correct HTTP status codes are returned for authorization failures
- Parameterized policies correctly extract and validate resource ownership

### What to Test
- ✅ Authorization policies (InstrumentViewer, InstrumentOwner)
- ✅ Family-based data isolation
- ✅ Resource ownership validation
- ✅ 401/403 response codes

### What NOT to Test
- ❌ Business logic (use unit tests)
- ❌ Data transformation (use unit tests)
- ❌ Happy-path CRUD operations (use E2E tests)

### Authorization Test Example

```csharp
/// <summary>
/// Integration tests for instrument authorization policies.
/// Verifies that users can only access instruments they own or have been shared with.
/// </summary>
public class InstrumentAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly TestAuthHandler _authHandler;

    private static readonly Guid OwnedInstrumentId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SharedInstrumentId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid UnauthorizedInstrumentId = new("33333333-3333-3333-3333-333333333333");

    public InstrumentAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _authHandler = new TestAuthHandler();
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        }).CreateClient();
    }

    /// <summary>
    /// Given I am authenticated as the instrument owner
    /// When I request GET for my owned instrument
    /// Then the response status should be 200 OK
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Owner_CanAccessOwnedInstrument()
    {
        // Arrange
        _authHandler.SetUser(TestModels.UserId, accounts: [OwnedInstrumentId]);

        // Act
        var response = await _client.GetAsync($"/api/v1/accounts/{OwnedInstrumentId}", TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated as a family member with shared access
    /// When I request GET for a shared instrument
    /// Then the response status should be 200 OK
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task FamilyMember_CanAccessSharedInstrument()
    {
        // Arrange
        _authHandler.SetUser(TestModels.FamilyUserId, sharedAccounts: [SharedInstrumentId]);

        // Act
        var response = await _client.GetAsync($"/api/v1/accounts/{SharedInstrumentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Given I am authenticated but do not own or have access to the instrument
    /// When I request GET for an unauthorized instrument
    /// Then the response status should be 403 Forbidden
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task User_CannotAccessUnauthorizedInstrument()
    {
        // Arrange
        _authHandler.SetUser(TestModels.UserId, accounts: [OwnedInstrumentId]);

        // Act
        var response = await _client.GetAsync($"/api/v1/accounts/{UnauthorizedInstrumentId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    /// <summary>
    /// Given I am not authenticated
    /// When I request GET for any instrument
    /// Then the response status should be 401 Unauthorized
    /// </summary>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Unauthenticated_ReceivesUnauthorized()
    {
        // Arrange
        _authHandler.SetUnauthenticated();

        // Act
        var response = await _client.GetAsync($"/api/v1/accounts/{OwnedInstrumentId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

## Running Tests

```bash
# Run all tests
dotnet test tests/

# Run unit tests only
dotnet test --filter /[Category=Unit]

# Run integration tests only
dotnet test --filter /[Category=Integration]


# Run with coverage
dotnet test tests/ --collect:"XPlat Code Coverage"
```

## Test Distribution

```
Unit Tests (80%)
├── Domain entity logic
├── Specification logic
├── Handler behavior
└── Complex calculations

Integration Tests (20%)
├── Authorization policies
├── Resource ownership
└── Family data isolation
```
