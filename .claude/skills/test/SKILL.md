---
name: test
description: "Run the test suite"
disable-model-invocation: true
---

# Run Tests

## All Tests

```bash
dotnet test tests/
```

## Filtered

```bash
# Unit tests only
dotnet test --filter /[Category=Unit]

# Integration tests only
dotnet test --filter /[Category=Integration]

# Specific test name
dotnet test tests/ --filter "FullyQualifiedName~YourTestName"
```

## Frontend Tests

```bash
cd src/Asm.MooBank.Web.App && npm test
```

## Report

- Total tests run
- Passed / Failed / Skipped
- Failed test details with error messages
