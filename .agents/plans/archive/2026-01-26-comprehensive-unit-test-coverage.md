# Feature: Comprehensive Unit Test Coverage for MooBank Core

The following plan should be complete, but its important that you validate documentation and codebase patterns and task sanity before you start implementing.

Pay special attention to naming of existing utils types and models. Import from the right files etc.

## Feature Description

Expand unit test coverage for MooBank core projects (Domain, Services, Security, Models) from the current ~59 tests to approximately 200+ tests, achieving 70-80% code coverage on business logic. The focus is on testing domain entity behavior, specifications, value objects, authorization handlers, and comparers.

## User Story

As a **developer maintaining MooBank**
I want to **have comprehensive unit test coverage**
So that **I can confidently refactor and extend the codebase knowing regressions will be caught**

## Problem Statement

Current test coverage is limited to 6 test files with ~59 tests covering only a subset of domain entities, one specification, one service, and partial security testing. Many critical business logic areas lack tests, including:
- Transaction filtering and sorting specifications
- Additional authorization handlers
- Value objects (StockSymbol)
- Equality comparers (TagEqualityComparer, TransactionComparer)
- LogicalAccount-specific business logic
- Tag entity equality

## Solution Statement

Implement comprehensive unit tests following the established xUnit patterns with Gherkin-style XML documentation. Tests will be organized by category (Domain, Specifications, Models, Security, Services) with each test file targeting specific classes or related functionality.

## Feature Metadata

**Feature Type**: Enhancement
**Estimated Complexity**: Medium-High
**Primary Systems Affected**: Asm.MooBank.Core.Tests
**Dependencies**: xUnit, Moq, Bogus (all already present)

---

## CONTEXT REFERENCES

### Relevant Codebase Files IMPORTANT: YOU MUST READ THESE FILES BEFORE IMPLEMENTING!

**Test Infrastructure (read first):**
- `tests/Asm.MooBank.Core.Tests/Support/TestBase.cs` - Base class for tests
- `tests/Asm.MooBank.Core.Tests/Support/TestEntities.cs` - Entity factories
- `tests/Asm.MooBank.Core.Tests/Support/TestModels.cs` - Static test data
- `tests/Asm.MooBank.Core.Tests/Support/Mocks.cs` - Mock infrastructure

**Existing Test Patterns (follow these patterns):**
- `tests/Asm.MooBank.Core.Tests/Domain/TransactionTests.cs` - Domain entity test pattern
- `tests/Asm.MooBank.Core.Tests/Models/QuarterTests.cs` - Value object test pattern
- `tests/Asm.MooBank.Core.Tests/Security/InstrumentAuthorizationTests.cs` - Auth test pattern
- `tests/Asm.MooBank.Core.Tests/Specifications/OpenAccessibleSpecificationTests.cs` - Spec test pattern

**Classes to Test:**
- `src/Asm.MooBank.Domain/Entities/Transactions/Specifications/FilterSpecification.cs` - Complex filtering
- `src/Asm.MooBank.Domain/Entities/Transactions/Specifications/SortSpecification.cs` - Dynamic sorting
- `src/Asm.MooBank.Domain/Entities/Transactions/TransactionComparer.cs` - Equality comparer
- `src/Asm.MooBank.Domain/Entities/Tag/Tag.cs` - Tag + TagEqualityComparer
- `src/Asm.MooBank.Domain/Entities/Account/LogicalAccount.cs` - Account logic
- `src/Asm.MooBank.Models/StockSymbol.cs` - Value object
- `src/Asm.MooBank.Security/Authorisation/InstrumentViewerAuthorisationHandler.cs` - Viewer auth
- `src/Asm.MooBank.Security/Authorisation/GroupOwnerAuthorisationHandler.cs` - Group auth

### New Files to Create

**Domain Tests:**
- `tests/Asm.MooBank.Core.Tests/Domain/TagTests.cs` - Tag entity and TagEqualityComparer
- `tests/Asm.MooBank.Core.Tests/Domain/LogicalAccountTests.cs` - LogicalAccount business logic

**Specification Tests:**
- `tests/Asm.MooBank.Core.Tests/Specifications/FilterSpecificationTests.cs` - Transaction filtering
- `tests/Asm.MooBank.Core.Tests/Specifications/SortSpecificationTests.cs` - Transaction sorting

**Model Tests:**
- `tests/Asm.MooBank.Core.Tests/Models/StockSymbolTests.cs` - StockSymbol value object

**Comparer Tests:**
- `tests/Asm.MooBank.Core.Tests/Comparers/TransactionComparerTests.cs` - Transaction equality
- `tests/Asm.MooBank.Core.Tests/Comparers/TagEqualityComparerTests.cs` - Tag equality

**Security Tests:**
- `tests/Asm.MooBank.Core.Tests/Security/ViewerAuthorizationTests.cs` - InstrumentViewer auth
- `tests/Asm.MooBank.Core.Tests/Security/GroupAuthorizationTests.cs` - Group owner auth

### Relevant Documentation YOU SHOULD READ BEFORE IMPLEMENTING!

- `.claude/reference/testing-backend.md` - Testing guidelines with required patterns
- `tests/Asm.MooBank.Core.Tests/Asm.MooBank.Core.Tests.csproj` - Project dependencies

### Patterns to Follow

**Test Class Documentation:**
```csharp
/// <summary>
/// Unit tests for the <see cref="ClassName"/> class.
/// Tests cover [brief description of test coverage areas].
/// </summary>
public class ClassNameTests
{
```

**Test Method Documentation (Gherkin-style):**
```csharp
/// <summary>
/// Given [precondition]
/// When [action under test]
/// Then [expected outcome]
/// </summary>
[Fact]
[Trait("Category", "Unit")]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange

    // Act

    // Assert
}
```

**Theory with InlineData:**
```csharp
[Theory]
[InlineData(value1, expectedResult1)]
[InlineData(value2, expectedResult2)]
[Trait("Category", "Unit")]
public void MethodName_Scenario_ExpectedBehavior(Type param, Type expected)
```

**Naming Convention:**
- Test class: `{ClassUnderTest}Tests`
- Test method: `{MethodName}_{Scenario}_{ExpectedBehavior}`

---

## IMPLEMENTATION PLAN

### Phase 1: Comparers (Simple, foundational)

Create tests for equality comparers used throughout the codebase.

**Tasks:**
- TransactionComparer tests (equality by time and amount)
- TagEqualityComparer tests (equality by ID)

### Phase 2: Value Objects

Expand value object testing with StockSymbol.

**Tasks:**
- StockSymbol parsing and validation tests
- Equality operator tests
- Edge case handling

### Phase 3: Domain Entities

Add coverage for domain entities with untested business logic.

**Tasks:**
- Tag entity equality tests
- LogicalAccount business logic tests

### Phase 4: Specifications

Add comprehensive tests for transaction specifications.

**Tasks:**
- FilterSpecification tests (all filter conditions)
- SortSpecification tests (dynamic sorting)

### Phase 5: Security

Complete authorization handler coverage.

**Tasks:**
- InstrumentViewerAuthorisationHandler tests
- GroupOwnerAuthorisationHandler tests

---

## STEP-BY-STEP TASKS

### PHASE 1: Comparers

#### Task 1.1: CREATE tests/Asm.MooBank.Core.Tests/Comparers/TransactionComparerTests.cs

- **IMPLEMENT**: Tests for TransactionComparer equality logic
- **PATTERN**: Follow `tests/Asm.MooBank.Core.Tests/Domain/TransactionTests.cs` structure
- **TESTS TO INCLUDE**:
  - `Equals_BothNull_ReturnsTrue`
  - `Equals_OneNull_ReturnsFalse`
  - `Equals_SameTimeAndAmount_ReturnsTrue`
  - `Equals_DifferentTime_ReturnsFalse`
  - `Equals_DifferentAmount_ReturnsFalse`
  - `GetHashCode_SameValues_ReturnsSameHash`
  - `GetHashCode_DifferentValues_ReturnsDifferentHash`
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~TransactionComparerTests"`

#### Task 1.2: CREATE tests/Asm.MooBank.Core.Tests/Comparers/TagEqualityComparerTests.cs

- **IMPLEMENT**: Tests for TagEqualityComparer
- **PATTERN**: Follow comparer test structure from Task 1.1
- **TESTS TO INCLUDE**:
  - `Equals_BothNull_ReturnsTrue`
  - `Equals_FirstNull_ReturnsFalse`
  - `Equals_SecondNull_ReturnsFalse`
  - `Equals_SameId_ReturnsTrue`
  - `Equals_DifferentId_ReturnsFalse`
  - `GetHashCode_ReturnsIdHashCode`
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~TagEqualityComparerTests"`

### PHASE 2: Value Objects

#### Task 2.1: CREATE tests/Asm.MooBank.Core.Tests/Models/StockSymbolTests.cs

- **IMPLEMENT**: Comprehensive StockSymbol value object tests
- **PATTERN**: Follow `tests/Asm.MooBank.Core.Tests/Models/QuarterTests.cs` structure
- **TESTS TO INCLUDE**:
  - **Parsing:**
    - `Parse_ValidSymbolWithExchange_ReturnsStockSymbol` ("AAPL.US")
    - `Parse_ValidSymbolWithoutExchange_ReturnsStockSymbol` ("AAPL")
    - `Parse_InvalidFormat_ThrowsFormatException` (too many dots)
    - `Parse_InvalidExchangeLength_ThrowsFormatException` (exchange not 2 chars)
    - `Parse_ConvertsToUpperCase_ReturnsUpperCaseSymbol`
  - **TryParse:**
    - `TryParse_ValidSymbol_ReturnsTrueWithResult`
    - `TryParse_InvalidSymbol_ReturnsFalse`
  - **ToString:**
    - `ToString_WithExchange_ReturnsFormattedString`
    - `ToString_WithoutExchange_ReturnsSymbolOnly`
  - **Equality:**
    - `Equals_SameSymbolAndExchange_ReturnsTrue`
    - `Equals_DifferentSymbol_ReturnsFalse`
    - `Equals_DifferentExchange_ReturnsFalse`
    - `Equals_NullOther_ReturnsFalse`
    - `OperatorEquals_SameValues_ReturnsTrue`
    - `OperatorNotEquals_DifferentValues_ReturnsTrue`
  - **Implicit Conversions:**
    - `ImplicitFromString_ParsesCorrectly`
    - `ImplicitToString_ReturnsToString`
  - **GetHashCode:**
    - `GetHashCode_SameValues_ReturnsSameHash`
- **GOTCHA**: StockSymbol.TryParse has a bug (line 37 accesses split[1] without length check). Test should handle this.
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~StockSymbolTests"`

### PHASE 3: Domain Entities

#### Task 3.1: CREATE tests/Asm.MooBank.Core.Tests/Domain/TagTests.cs

- **IMPLEMENT**: Tests for Tag entity equality logic
- **PATTERN**: Follow `tests/Asm.MooBank.Core.Tests/Domain/TransactionTests.cs` structure
- **IMPORTS**: `using Asm.MooBank.Domain.Entities.Tag;`
- **TESTS TO INCLUDE**:
  - `Equals_SameId_ReturnsTrue`
  - `Equals_DifferentId_ReturnsFalse`
  - `Equals_NullOther_ReturnsFalse`
  - `Equals_ObjectSameId_ReturnsTrue`
  - `Equals_ObjectDifferentType_ReturnsFalse`
  - `OperatorEquals_BothNull_ReturnsTrue`
  - `OperatorEquals_LeftNull_ReturnsFalse`
  - `OperatorEquals_RightNull_ReturnsFalse`
  - `OperatorEquals_SameId_ReturnsTrue`
  - `OperatorNotEquals_DifferentId_ReturnsTrue`
  - `GetHashCode_ReturnsIdHashCode`
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~TagTests"`

#### Task 3.2: CREATE tests/Asm.MooBank.Core.Tests/Domain/LogicalAccountTests.cs

- **IMPLEMENT**: Tests for LogicalAccount business logic
- **PATTERN**: Follow existing domain test patterns
- **IMPORTS**: `using Asm.MooBank.Domain.Entities.Account;`, `using Asm.MooBank.Domain.Entities.Instrument;`
- **PREREQUISITE**: Update TestEntities.cs to expose helper methods for creating LogicalAccount with Viewers
- **TESTS TO INCLUDE**:
  - **ValidViewers:**
    - `ValidViewers_ShareWithFamilyFalse_ReturnsEmpty`
    - `ValidViewers_ShareWithFamilyTrue_ReturnsViewersInSameFamily`
    - `ValidViewers_ShareWithFamilyTrue_ExcludesViewersFromDifferentFamily`
  - **AddInstitutionAccount:**
    - `AddInstitutionAccount_AddsToCollection`
  - **GetGroup:**
    - `GetGroup_ForOwner_ReturnsOwnerGroup`
    - `GetGroup_ForValidViewer_ReturnsViewerGroup`
    - `GetGroup_ForNonMember_ReturnsNull`
  - **SetController:**
    - `SetController_ToManual_ClearsImporterTypeOnInstitutionAccounts`
    - `SetController_ToImport_KeepsImporterType`
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~LogicalAccountTests"`

### PHASE 4: Specifications

#### Task 4.1: UPDATE tests/Asm.MooBank.Core.Tests/Support/TestEntities.cs

- **IMPLEMENT**: Add helper methods needed for specification tests
- **ADD**: `CreateTransactionWithDescription(string description, decimal amount)`
- **ADD**: `CreateTransactionWithTags(decimal amount, params int[] tagIds)`
- **ADD**: `CreateTransactionAtTime(DateTime time, decimal amount)`
- **VALIDATE**: `dotnet build tests/Asm.MooBank.Core.Tests/`

#### Task 4.2: CREATE tests/Asm.MooBank.Core.Tests/Specifications/FilterSpecificationTests.cs

- **IMPLEMENT**: Comprehensive FilterSpecification tests
- **PATTERN**: Follow `OpenAccessibleSpecificationTests.cs` structure
- **IMPORTS**: `using Asm.MooBank.Domain.Entities.Transactions.Specifications;`, `using Asm.MooBank.Models;`
- **TESTS TO INCLUDE**:
  - **InstrumentId Filter:**
    - `Apply_FiltersToSpecifiedInstrumentId`
  - **Description Filter:**
    - `Apply_WithSingleFilter_ReturnsMatchingDescriptions`
    - `Apply_WithCommaDelimitedFilters_ReturnsAnyMatching`
    - `Apply_WithNoFilter_ReturnsAll`
  - **TransactionType Filter:**
    - `Apply_WithDebitFilter_ReturnsOnlyDebits`
    - `Apply_WithCreditFilter_ReturnsOnlyCredits`
    - `Apply_WithNoneFilter_ReturnsAll`
  - **Date Range Filter:**
    - `Apply_WithStartDate_ReturnsOnlyAfterStart`
    - `Apply_WithEndDate_ReturnsOnlyBeforeEnd`
    - `Apply_WithDateRange_ReturnsWithinRange`
  - **UntaggedOnly Filter:**
    - `Apply_WithUntaggedOnly_ReturnsOnlyUntagged`
  - **TagIds Filter:**
    - `Apply_WithTagIds_ReturnsOnlyMatchingTags`
- **GOTCHA**: FilterSpecification uses EF.Functions.Like which requires special handling in unit tests. Use in-memory provider or mock appropriately.
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~FilterSpecificationTests"`

#### Task 4.3: CREATE tests/Asm.MooBank.Core.Tests/Specifications/SortSpecificationTests.cs

- **IMPLEMENT**: SortSpecification tests for dynamic sorting
- **PATTERN**: Follow specification test structure
- **TESTS TO INCLUDE**:
  - **Default Sort:**
    - `Apply_WithNullField_SortsByTransactionTimeAscending`
    - `Apply_WithEmptyField_SortsByTransactionTimeAscending`
  - **Named Field Sort:**
    - `Apply_WithAmountField_SortsByAbsoluteAmount`
    - `Apply_WithDescriptionField_SortsByDescription`
  - **Sort Direction:**
    - `Apply_WithAscendingDirection_SortsAscending`
    - `Apply_WithDescendingDirection_SortsDescending`
  - **Nested Property Sort:**
    - `Apply_WithNestedProperty_SortsByNestedValue` (e.g., "Splits.Amount")
- **GOTCHA**: SortSpecification uses expression trees and reflection. Tests need IQueryable not just IEnumerable.
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~SortSpecificationTests"`

### PHASE 5: Security

#### Task 5.1: CREATE tests/Asm.MooBank.Core.Tests/Security/ViewerAuthorizationTests.cs

- **IMPLEMENT**: InstrumentViewerAuthorisationHandler logic tests
- **PATTERN**: Follow `InstrumentAuthorizationTests.cs` structure
- **TESTS TO INCLUDE**:
  - `ViewerAuthorization_ForOwnedInstrument_Succeeds`
  - `ViewerAuthorization_ForSharedInstrument_Succeeds`
  - `ViewerAuthorization_ForUnauthorizedInstrument_Fails`
  - `ViewerAuthorization_ForNullUser_Fails`
  - `ViewerAuthorization_WithInvalidGuid_Fails`
- **NOTE**: These tests replicate the handler logic in isolation, similar to existing InstrumentAuthorizationTests
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~ViewerAuthorizationTests"`

#### Task 5.2: CREATE tests/Asm.MooBank.Core.Tests/Security/GroupAuthorizationTests.cs

- **IMPLEMENT**: GroupOwnerAuthorisationHandler logic tests
- **PATTERN**: Follow authorization test structure
- **PREREQUISITE**: Update TestModels.cs to add GroupId constant
- **TESTS TO INCLUDE**:
  - `GroupOwnerAuthorization_ForOwnedGroup_Succeeds`
  - `GroupOwnerAuthorization_ForNonOwnedGroup_Fails`
  - `GroupOwnerAuthorization_ForNullUser_Fails`
  - `GroupOwnerAuthorization_WithInvalidGuid_Fails`
- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~GroupAuthorizationTests"`

### PHASE 6: Final Validation

#### Task 6.1: Run Full Test Suite

- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --verbosity normal`
- **EXPECTED**: All tests pass (150+ tests)

#### Task 6.2: Run Code Coverage

- **VALIDATE**: `dotnet test tests/Asm.MooBank.Core.Tests/ --collect:"XPlat Code Coverage"`
- **EXPECTED**: Coverage report generated in TestResults folder

---

## TESTING STRATEGY

### Unit Tests

All tests follow these conventions:
- xUnit test framework with `[Fact]` and `[Theory]` attributes
- `[Trait("Category", "Unit")]` on all tests
- Gherkin-style XML documentation (Given/When/Then)
- Arrange-Act-Assert structure with comments
- Descriptive naming: `{Method}_{Scenario}_{Expected}`

### Test Data

Use existing infrastructure:
- `TestEntities` for creating domain entities with Bogus
- `TestModels` for static test data (GUIDs, etc.)
- `Mocks` for mocked dependencies

### Edge Cases to Test

- Null inputs (comparers, authorization handlers)
- Empty collections (filters, viewers)
- Invalid GUIDs (authorization handlers)
- Boundary conditions (date ranges, amounts)
- Case sensitivity (StockSymbol parsing)

---

## VALIDATION COMMANDS

### Level 1: Build
```bash
dotnet build tests/Asm.MooBank.Core.Tests/
```

### Level 2: Run All Unit Tests
```bash
dotnet test tests/Asm.MooBank.Core.Tests/ --verbosity normal
```

### Level 3: Run Specific Test Categories
```bash
# Comparers
dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~ComparerTests"

# Models
dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~Models"

# Domain
dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~Domain"

# Security
dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~Security"

# Specifications
dotnet test tests/Asm.MooBank.Core.Tests/ --filter "FullyQualifiedName~Specification"
```

### Level 4: Code Coverage
```bash
dotnet test tests/Asm.MooBank.Core.Tests/ --collect:"XPlat Code Coverage"
```

---

## ACCEPTANCE CRITERIA

- [ ] All existing 84 tests continue to pass
- [ ] At least 150 total tests in Asm.MooBank.Core.Tests
- [ ] All new test files follow Gherkin-style documentation
- [ ] All tests use `[Trait("Category", "Unit")]`
- [ ] TransactionComparer has 7+ tests
- [ ] TagEqualityComparer has 6+ tests
- [ ] StockSymbol has 15+ tests
- [ ] Tag entity has 10+ tests
- [ ] LogicalAccount has 8+ tests
- [ ] FilterSpecification has 10+ tests
- [ ] SortSpecification has 6+ tests
- [ ] Viewer authorization has 5+ tests
- [ ] Group authorization has 4+ tests
- [ ] All validation commands pass
- [ ] No build warnings in test project

---

## COMPLETION CHECKLIST

- [ ] Phase 1 complete: Comparer tests created and passing
- [ ] Phase 2 complete: StockSymbol tests created and passing
- [ ] Phase 3 complete: Tag and LogicalAccount tests created and passing
- [ ] Phase 4 complete: FilterSpecification and SortSpecification tests created and passing
- [ ] Phase 5 complete: Viewer and Group authorization tests created and passing
- [ ] Full test suite passes (150+ tests)
- [ ] All tests have proper Gherkin documentation
- [ ] No linting errors
- [ ] Code coverage improved toward 70-80% target

---

## NOTES

### Design Decisions

1. **Testing Authorization Logic Directly**: Rather than testing the full ASP.NET Core authorization pipeline (which requires WebApplicationFactory), tests verify the core authorization logic in isolation by replicating the handler's IsAuthorised method. This is consistent with the existing InstrumentAuthorizationTests approach.

2. **Specification Testing with In-Memory Data**: FilterSpecification and SortSpecification tests use in-memory collections converted to IQueryable. This tests the logic without requiring a real database, though EF.Functions.Like won't work exactly as in production.

3. **StockSymbol TryParse Bug**: The existing code has a bug in TryParse (accesses split[1] without checking length). Tests should document this behavior.

### Test Count Estimates

| Category | New Tests | Total After |
|----------|-----------|-------------|
| Comparers | 13 | 13 |
| Models | 18 | 42 (24 existing) |
| Domain | 21 | 54 (33 existing) |
| Security | 9 | 16 (7 existing) |
| Specifications | 16 | 20 (4 existing) |
| Services | 0 | 4 (existing) |
| **TOTAL** | **77** | **149** |

### Risk Considerations

1. **FilterSpecification uses EF.Functions.Like**: This won't work with in-memory IQueryable. May need to use AsEnumerable() for some tests or accept that description filtering tests have limitations.

2. **SortSpecification uses reflection**: Tests must use IQueryable<Transaction>, not just enumerable, for the expression tree building to work.

3. **LogicalAccount requires complex setup**: Tests need Viewers and Owners with User.FamilyId set correctly. May need to expand TestEntities.
