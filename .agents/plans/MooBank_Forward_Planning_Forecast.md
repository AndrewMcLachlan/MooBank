# MooBank – Forward Planning (Cashflow Forecast) Feature Plan

This document defines the Forward Planning / Cashflow Forecast feature for MooBank.
It is intended to be consumed by an automated coding agent (e.g. GitHub Copilot, Claude Code)
that already understands the existing MooBank codebase, architecture, and conventions.

The document specifies what to build, not how to write the code.

Data and JSON schema definitions are examples only. Always prefer the guidelines laid out in @AGENTS.md

---

## 1. Feature Goal

Provide a forecasting engine that projects a user’s financial position forward in time
(up to 10 years), using:

- existing balances / savings
- expected income
- baseline outgoings inferred from historical data
- planned future expenses and income
- derived insights such as required monthly savings

The output is a month-by-month projection with summary metrics and drill-down capability.

---

## 2. Core Concepts

### 2.1 Forecast Plan

A Forecast Plan is a scenario definition:
“What happens if I keep earning X, spend like I usually do, and have these future expenses?”

Plans:
- are independent of real transactions
- can be duplicated to compare scenarios
- do not mutate the actual ledger

---

## 3. Domain Model

### 3.1 ForecastPlan

Container for all assumptions, scope, and rules.

Key responsibilities:
- define time horizon (start → end, max 10 years)
- define which accounts are included
- define how starting balance is calculated
- reference income and outgoing strategies
- hold global modelling assumptions

---

### 3.2 PlannedItem

Represents future income or expense that does not yet exist as a transaction.

Supports:
- fixed-date items
- recurring scheduled items
- flexible “sometime within a window” items

Used for:
- large one-off purchases
- future bills
- bonuses or irregular income
- planning scenarios

---

### 3.3 Strategies

Strategies describe how values are derived instead of storing raw numbers.

- IncomeStrategy: how future income is calculated
- OutgoingStrategy: how baseline outgoings are inferred
- Assumptions: inflation, buffers, modelling flags

All strategies are stored as versioned JSON blobs.

---

## 4. Database Schema (SQL Server)

### 4.1 ForecastPlan

ForecastPlan
- ForecastPlanId (PK, uniqueidentifier)
- Name (nvarchar(200))
- StartDate (date)
- EndDate (date)
- AccountScopeMode (tinyint)
  - 0 = All accounts
  - 1 = Selected accounts
- StartingBalanceMode (tinyint)
  - 0 = Use calculated current balances
  - 1 = Manual amount
  - 2 = Virtual account balances (future)
- StartingBalanceAmount (decimal(18,2), null)
- CurrencyCode (char(3), null)

- IncomeStrategyJson (nvarchar(max))
- OutgoingStrategyJson (nvarchar(max))
- AssumptionsJson (nvarchar(max))

- IsArchived (bit)

- CreatedUtc
- UpdatedUtc
- UserId

Constraints:
- EndDate must be <= StartDate + 10 years

---

### 4.2 ForecastPlanAccount

Used when account scope is restricted.

ForecastPlanAccount
- ForecastPlanAccountId (PK)
- ForecastPlanId (FK)
- AccountId (FK)

Unique:
- (ForecastPlanId, AccountId)

---

### 4.3 ForecastPlannedItem

ForecastPlannedItem
- PlannedItemId (PK)
- ForecastPlanId (FK)

- ItemType (tinyint)
  - 0 = Expense
  - 1 = Income

- Name (nvarchar(200))
- Amount (decimal(18,2)) -- always positive
- TagId (uniqueidentifier, null)
- VirtualAccountId (uniqueidentifier, null)

- IsIncluded (bit)

- DateMode (tinyint)
  - 0 = FixedDate
  - 1 = Schedule
  - 2 = FlexibleWindow

Fixed date fields:
- FixedDate (date, null)

Schedule fields:
- ScheduleFrequency (tinyint, null)
- ScheduleAnchorDate (date, null)
- ScheduleInterval (int, null)
- ScheduleDayOfMonth (int, null)
- ScheduleEndDate (date, null)

Flexible window fields:
- WindowStartDate (date, null)
- WindowEndDate (date, null)
- AllocationMode (tinyint, null)
  - 0 = EvenlySpread
  - 1 = AllAtEnd

- Notes (nvarchar(max), null)

---

### 4.4 Optional Cached Forecast Results

ForecastRun
- ForecastRunId (PK)
- ForecastPlanId (FK)
- RunAtUtc (datetime2)
- InputsHash (varchar(64))
- TransactionWatermark (datetime2)
- Status (tinyint)
- Error (nvarchar(max), null)

ForecastRunMonth
- ForecastRunMonthId (PK)
- ForecastRunId (FK)
- MonthStart (date)

- OpeningBalance (decimal(18,2))
- IncomeTotal (decimal(18,2))
- BaselineOutgoingsTotal (decimal(18,2))
- PlannedItemsTotal (decimal(18,2))
- ClosingBalance (decimal(18,2))

ForecastRunMonthBreakdown (optional)
- ForecastRunMonthBreakdownId (PK)
- ForecastRunMonthId (FK)
- Category (tinyint)
  - 0 = Income
  - 1 = BaselineOutgoing
  - 2 = Planned
- TagId (uniqueidentifier, null)
- Amount (decimal(18,2))

---

## 5. Strategy JSON Schemas

### 5.1 IncomeStrategy (v1)

{
  "version": 1,
  "mode": "ManualRecurring",
  "manualRecurring": {
    "amount": 12000.00,
    "frequency": "Monthly",
    "startDate": "2026-01-01",
    "endDate": null
  },
  "manualAdjustments": [
    { "date": "2026-07-01", "deltaAmount": 500.00 }
  ],
  "historical": {
    "lookbackMonths": 12,
    "includeTagIds": [],
    "excludeTagIds": [],
    "excludeTransfers": true,
    "excludeOffsets": false
  }
}

---

### 5.2 OutgoingStrategy (v1)

{
  "version": 1,
  "mode": "HistoricalAverageByTag",
  "lookbackMonths": 12,
  "excludeTagIds": [],
  "excludeTransfers": true,
  "excludeOffsets": true,
  "excludeAboveAmount": 2000.00,
  "seasonality": { "enabled": false }
}

---

### 5.3 Assumptions (v1)

{
  "version": 1,
  "inflationRateAnnual": null,
  "applyInflationToBaseline": false,
  "applyInflationToPlanned": false,
  "safetyBuffer": 0.00
}

---

## 6. Forecast Computation Model

### 6.1 Timeline

- Monthly buckets
- From StartDate month to EndDate month inclusive
- Maximum 120 months

---

### 6.2 Baseline Outgoings

Derived from historical transactions:
- bounded by lookback window
- filtered by account scope
- excludes transfers
- optionally excludes offset-linked transactions
- optional exclusion of large one-off amounts
- aggregated to a monthly average

---

### 6.3 Planned Item Expansion

- FixedDate: allocated to month containing the date
- Schedule: occurrences generated from anchor to plan end
- FlexibleWindow:
  - EvenlySpread: divided equally across months in window
  - AllAtEnd: allocated to month containing window end

---

### 6.4 Monthly Roll-Forward

balance = startingBalance

for each month:
- income = incomeForMonth
- baseline = baselineOutgoingsForMonth
- planned = plannedItemsForMonth
- closingBalance = balance + income - baseline + planned
- balance = closingBalance

---

## 7. Derived Metrics

### 7.1 Minimum Required Monthly Uplift

- Identify the lowest projected balance
- If lowest >= 0: uplift = 0
- Else:
  - monthsUntilLow = number of months from start to lowest point
  - upliftPerMonth = abs(lowestBalance) / monthsUntilLow

Used to answer:
“How much extra must be saved per month to never go negative?”

---

### 7.2 Sinking-Fund Style Saving

For selected planned expenses:
- monthsUntilDue = months between start and due month
- monthlyContribution = amount / monthsUntilDue

Summed to produce a recommended saving per month for planned expenses.

---

## 8. API Surface (Suggested)

Plans
- GET /api/forecast/plans
- POST /api/forecast/plans
- GET /api/forecast/plans/{planId}
- PUT /api/forecast/plans/{planId}
- DELETE /api/forecast/plans/{planId}

Accounts
- PUT /api/forecast/plans/{planId}/accounts

Planned Items
- GET /api/forecast/plans/{planId}/items
- POST /api/forecast/plans/{planId}/items
- PUT /api/forecast/plans/{planId}/items/{itemId}
- DELETE /api/forecast/plans/{planId}/items/{itemId}

Execution
- POST /api/forecast/plans/{planId}/run
- GET /api/forecast/runs/{runId}

---

## 9. UX Overview

### 9.1 Forecast Plans List
- Name
- Date range
- Account scope
- Last run time
- Actions: Create, Duplicate, Archive

### 9.2 Plan Editor
Sections:
- Basics (name, dates, accounts, starting balance)
- Income strategy
- Outgoing baseline strategy
- Planned items list
- Run forecast action

### 9.3 Forecast Results
- Line chart of projected balance
- Highlight lowest balance point
- Summary cards:
  - Lowest balance
  - Required monthly uplift
  - Months below zero
- Expandable monthly breakdown table

---

## 10. MVP Scope

- ForecastPlan CRUD
- PlannedItem: fixed-date and monthly schedule
- Manual recurring income
- Historical baseline outgoings (total only)
- Monthly forecast table and balance chart
- Required monthly uplift metric

---

## 11. V1 Enhancements

- Baseline outgoings by tag
- Flexible window allocation
- Scenario comparison
- Virtual account sinking funds
- Cached forecast runs
