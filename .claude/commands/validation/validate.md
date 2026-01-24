# Validate

Run full validation suite on the codebase.

## Instructions

Execute in order, stopping on first failure:

### 1. Backend Build
```bash
dotnet build Asm.MooBank.slnx --no-restore
```

### 2. Frontend Linting
```bash
cd src/Asm.MooBank.Web.App && npm run lint
```

### 3. Frontend Type Checking & Build
```bash
cd src/Asm.MooBank.Web.App && npm run build
```
Note: `npm run build` runs `tsc && vite build`, so this covers type checking.

### 4. Backend Tests
```bash
dotnet test tests/ --no-build
```

## 5. Summary Report

After all validations complete, provide a summary report with:

- Linting status
- Tests passed/failed
- Coverage percentage
- Frontend build status
- Any errors or warnings encountered
- Overall health assessment (PASS/FAIL)

**Format the report clearly with sections and status indicators**

## Quick Validation

For faster iteration during development, you can run just:
- Backend: `dotnet build Asm.MooBank.slnx`
- Frontend: `cd src/Asm.MooBank.Web.App && npm run build`
