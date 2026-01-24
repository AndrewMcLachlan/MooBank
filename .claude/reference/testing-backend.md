# Backend Testing Guidelines

## Overview

Backend testing uses ReqnRoll (BDD) for both unit and integration tests:
- **Unit Tests** - Domain logic, handlers, specifications
- **Integration Tests** - Authorization policy testing only

## Unit Tests with ReqnRoll

### Location
```
tests/Asm.MooBank.Tests/
```

### Framework
- **ReqnRoll** - BDD test framework (Gherkin syntax)
- **xUnit** - Underlying test runner
- **Moq** or **NSubstitute** - Mocking

### What to Test
- Domain entity business logic and invariants
- Specification logic
- Command/Query handlers (with mocked repositories)
- Complex calculations (e.g., balance computations, streak calculations)

### Feature File Structure
```
tests/Asm.MooBank.Tests/
├── Features/
│   ├── Transactions/
│   │   ├── TransactionSplitting.feature
│   │   └── TransactionTagging.feature
│   ├── Budgets/
│   │   └── BudgetCalculation.feature
│   └── Instruments/
│       └── BalanceCalculation.feature
├── StepDefinitions/
│   ├── TransactionSteps.cs
│   ├── BudgetSteps.cs
│   └── InstrumentSteps.cs
└── Hooks/
    └── TestHooks.cs
```

### Example Feature File
```gherkin
# Features/Transactions/TransactionSplitting.feature
Feature: Transaction Splitting
    As a user
    I want to split a transaction across multiple categories
    So that I can accurately categorize mixed purchases

Scenario: Split transaction with valid amounts
    Given a transaction of $100.00 with description "Costco Shopping"
    When I split the transaction into:
        | Amount | Tag       |
        | 60.00  | Groceries |
        | 40.00  | Household |
    Then the transaction should have 2 splits
    And the split amounts should total $100.00

Scenario: Split transaction with amounts exceeding total
    Given a transaction of $100.00 with description "Shopping"
    When I attempt to split the transaction into:
        | Amount | Tag       |
        | 80.00  | Groceries |
        | 50.00  | Household |
    Then the split should fail with error "Split amounts exceed transaction total"

Scenario: Split transaction with partial amount
    Given a transaction of $100.00 with description "Mixed Purchase"
    When I split the transaction into:
        | Amount | Tag       |
        | 60.00  | Groceries |
    Then the transaction should have 2 splits
    And there should be an unallocated split of $40.00
```

### Example Step Definitions
```csharp
// StepDefinitions/TransactionSteps.cs
using ReqnRoll;

[Binding]
public class TransactionSteps
{
    private Transaction _transaction;
    private Exception _exception;

    [Given(@"a transaction of \$(.+) with description ""(.+)""")]
    public void GivenATransaction(decimal amount, string description)
    {
        _transaction = new Transaction(amount, description);
    }

    [When(@"I split the transaction into:")]
    public void WhenISplitTheTransactionInto(Table table)
    {
        var splits = table.Rows.Select(row => new SplitRequest(
            decimal.Parse(row["Amount"]),
            row["Tag"]
        )).ToList();

        try
        {
            _transaction.Split(splits);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [Then(@"the transaction should have (\d+) splits")]
    public void ThenTheTransactionShouldHaveSplits(int count)
    {
        Assert.Equal(count, _transaction.Splits.Count);
    }

    [Then(@"the split amounts should total \$(.+)")]
    public void ThenTheSplitAmountsShouldTotal(decimal total)
    {
        Assert.Equal(total, _transaction.Splits.Sum(s => s.Amount));
    }

    [Then(@"the split should fail with error ""(.+)""")]
    public void ThenTheSplitShouldFailWithError(string errorMessage)
    {
        Assert.NotNull(_exception);
        Assert.Contains(errorMessage, _exception.Message);
    }
}
```

### Handler Testing with ReqnRoll
```gherkin
# Features/Transactions/CreateTransaction.feature
Feature: Create Transaction Handler
    As a system
    I want to create transactions via the handler
    So that transactions are persisted correctly

Scenario: Successfully create a transaction
    Given a valid instrument exists with ID "abc-123"
    And a create transaction command with amount $50.00
    When the create transaction handler is executed
    Then the transaction should be added to the repository
    And the unit of work should be saved

Scenario: Create transaction for non-existent instrument
    Given no instrument exists with ID "invalid-id"
    And a create transaction command for instrument "invalid-id"
    When the create transaction handler is executed
    Then a NotFoundException should be thrown
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

### Example Authorization Feature
```gherkin
# Features/Authorization/InstrumentAuthorization.feature
Feature: Instrument Authorization
    As a system
    I want to enforce instrument access policies
    So that users can only access their own instruments

Scenario: User can access their own instrument
    Given I am authenticated as user "user-1"
    And user "user-1" owns instrument "instrument-abc"
    When I request GET "/api/instruments/instrument-abc"
    Then the response status should be 200 OK

Scenario: User cannot access another user's instrument
    Given I am authenticated as user "user-1"
    And user "user-2" owns instrument "instrument-xyz"
    When I request GET "/api/instruments/instrument-xyz"
    Then the response status should be 403 Forbidden

Scenario: Unauthenticated user cannot access instruments
    Given I am not authenticated
    When I request GET "/api/instruments/instrument-abc"
    Then the response status should be 401 Unauthorized

Scenario: Family member can view shared instrument
    Given I am authenticated as user "user-1"
    And user "user-1" is in family "family-a"
    And instrument "shared-instrument" is shared with family "family-a"
    When I request GET "/api/instruments/shared-instrument"
    Then the response status should be 200 OK

Scenario: User cannot modify instrument they can only view
    Given I am authenticated as user "user-1"
    And user "user-1" has viewer access to instrument "view-only-instrument"
    When I request PATCH "/api/instruments/view-only-instrument"
    Then the response status should be 403 Forbidden
```

### Authorization Step Definitions
```csharp
// StepDefinitions/AuthorizationSteps.cs
[Binding]
public class AuthorizationSteps
{
    private readonly HttpClient _client;
    private readonly TestAuthHandler _authHandler;
    private HttpResponseMessage _response;

    public AuthorizationSteps(WebApplicationFactory<Program> factory)
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

    [Given(@"I am authenticated as user ""(.+)""")]
    public void GivenIAmAuthenticatedAsUser(string userId)
    {
        _authHandler.SetUser(userId);
    }

    [Given(@"I am not authenticated")]
    public void GivenIAmNotAuthenticated()
    {
        _authHandler.SetUnauthenticated();
    }

    [When(@"I request (GET|POST|PATCH|DELETE) ""(.+)""")]
    public async Task WhenIRequest(string method, string path)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        _response = await _client.SendAsync(request);
    }

    [Then(@"the response status should be (\d+) (.+)")]
    public void ThenTheResponseStatusShouldBe(int statusCode, string statusName)
    {
        Assert.Equal(statusCode, (int)_response.StatusCode);
    }
}
```

## Running Tests

```bash
# Run all tests
dotnet test tests/

# Run specific feature
dotnet test tests/ --filter "FullyQualifiedName~TransactionSplitting"

# Run authorization tests only
dotnet test tests/ --filter "FullyQualifiedName~Authorization"

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
