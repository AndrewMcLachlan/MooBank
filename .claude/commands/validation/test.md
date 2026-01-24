# Run Tests

Execute the test suite to validate code changes.

## Instructions

### Run All Tests
```bash
dotnet test tests/
```

### Run Specific Test Project
```bash
dotnet test tests/Asm.MooBank.Tests/
```

### Run Tests with Filter
```bash
dotnet test tests/ --filter "FullyQualifiedName~YourTestName"
```

### Run BDD Tests (ReqnRoll)
BDD tests are located in test projects with `.feature` files.
```bash
dotnet test tests/ --filter "Category=BDD"
```

## Test Patterns

### Unit Tests
- Located in `tests/` directory
- Mirror the structure of main projects
- Use mocks and fakes for external dependencies

### BDD/Integration Tests
- Use ReqnRoll framework
- `.feature` files contain human-readable specifications
- Step definitions in corresponding `Steps/` folders

## Output

Report test results:
- Total tests run
- Passed / Failed / Skipped counts
- Failed test details with error messages
