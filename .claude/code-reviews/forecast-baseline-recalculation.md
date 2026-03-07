# Code Review: Forecast Baseline Recalculation from Actuals

**Date:** 2026-02-14
**Branch:** main (unstaged changes)

## Stats

- **Files Modified:** 2 (excluding package.json/lock changes)
- **Lines Added:** ~105
- **Lines Removed:** ~8

## Files Reviewed

| File | Status |
|------|--------|
| `src/Asm.MooBank.Modules.Forecast/Services/ForecastEngine.cs` | Modified |
| `tests/Asm.MooBank.Modules.Forecast.Tests/Services/ForecastEngineTests.cs` | Modified |

---

## Issues

### 1. Negative actual outgoings treated as valid baseline

```
severity: medium
file: src/Asm.MooBank.Modules.Forecast/Services/ForecastEngine.cs
line: 309
issue: Derived actual outgoings can be negative, producing a nonsensical baseline
detail: |
  The formula `opening + income + planned - closing` can produce a negative number
  if the actual closing balance grew more than predicted income + planned items would
  explain (e.g. unexpected large deposit, inter-account transfer). A negative "outgoings"
  value would flip the sign in `Math.Abs(effectiveBaselineOutgoings)` at line 82,
  effectively adding to the balance instead of subtracting. This could produce wildly
  optimistic forecasts.
suggestion: |
  Clamp derived outgoings to a minimum of 0:
    actualOutgoings.Add(Math.Max(0m, opening.Value + income + planned - closing.Value));
  Or skip months where the derived value is negative (likely anomalous data).
```

### 2. Predicted income used to derive "actual" outgoings

```
severity: low
file: src/Asm.MooBank.Modules.Forecast/Services/ForecastEngine.cs
line: 303-309
issue: Actual outgoings derivation depends on predicted income, not actual income
detail: |
  The formula uses `incomeByMonth` which contains predicted/configured income, not
  actual income from transactions. If the income prediction is inaccurate (e.g. user
  configured 5000 but actually received 6000), the derived outgoings will absorb that
  error. This is an inherent limitation of deriving outgoings from balance changes
  without separate actual income data, but worth documenting.
suggestion: |
  Add a comment in the method documenting this assumption, e.g.:
  "Note: uses predicted income as actual income data is not separately available.
  Inaccuracies in income prediction will be reflected in the derived outgoings."
  This is informational - no code change required unless actual income data becomes available.
```

### 3. No test for negative/anomalous derived outgoings

```
severity: low
file: tests/Asm.MooBank.Modules.Forecast.Tests/Services/ForecastEngineTests.cs
line: 790-979
issue: No test covers the case where actual balance grows faster than predicted, producing negative derived outgoings
detail: |
  If actual closing balance is much higher than opening + income + planned (e.g. large
  unexpected deposit), the derived outgoings would be negative. There's no test verifying
  the behavior in this edge case.
suggestion: |
  Add a test where actual_closing > actual_opening + income + planned to verify
  the resulting baseline is sensible. This pairs with issue #1 - once a clamping
  strategy is chosen, add a test for it.
```

## Summary

The core logic is sound - deriving baseline outgoings from actual balance changes is a reasonable approach and the implementation is clean. The main concern is **issue #1**: negative derived outgoings flipping the sign in the balance calculation. This should be addressed before shipping. Issues #2 and #3 are informational/minor.

All 95 existing tests pass. Code compiles with 0 warnings. Naming conventions and patterns are followed correctly.
