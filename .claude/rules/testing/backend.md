---
paths:
  - "tests/**/*.cs"
---

# Backend Testing Guidelines

## Overview

Backend testing uses xUnit with XML documentation comments in Gherkin style:
- **Unit Tests** - Domain logic, handlers, specifications
- **Integration Tests** - Authorization policy testing only

## Unit Tests

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
- Authorization policies (InstrumentViewer, InstrumentOwner)
- Family-based data isolation
- Resource ownership validation
- 401/403 response codes

### What NOT to Test
- Business logic (use unit tests)
- Data transformation (use unit tests)
- Happy-path CRUD operations (use E2E tests)

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
