---
name: validate
description: "Run full validation suite (build, lint, type check, tests)"
disable-model-invocation: true
---

# Validate

Run full validation suite on the codebase. Execute in order, stopping on first failure.

## Steps

### 1. Backend Build

```bash
dotnet build Asm.MooBank.slnx --no-restore
```

### 2. Frontend Lint

```bash
cd src/Asm.MooBank.Web.App && npm run lint
```

### 3. Frontend Build (includes type checking)

```bash
cd src/Asm.MooBank.Web.App && npm run build
```

### 4. Backend Tests

```bash
dotnet test tests/ --no-build
```

## Summary Report

After all validations complete, provide:

- Backend build: PASS/FAIL
- Frontend lint: PASS/FAIL
- Frontend build: PASS/FAIL
- Backend tests: PASS/FAIL (X passed, Y failed)
- Any warnings encountered
- **Overall: PASS/FAIL**
