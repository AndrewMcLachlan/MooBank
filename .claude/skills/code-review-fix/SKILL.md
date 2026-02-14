---
name: code-review-fix
description: "Fix bugs found in a code review"
argument-hint: [review-file-or-description]
---

# Fix Code Review Issues

Code review input: $ARGUMENTS

If the input is a file path, read the entire file first to understand all issues.

## Process

For each issue:

1. **Explain** what was wrong
2. **Fix** the issue
3. **Test** - create and run relevant tests to verify the fix

## After All Fixes

Run full validation to confirm no regressions:

```bash
dotnet build Asm.MooBank.slnx --no-restore
cd src/Asm.MooBank.Web.App && npm run build
dotnet test tests/ --no-build
```
