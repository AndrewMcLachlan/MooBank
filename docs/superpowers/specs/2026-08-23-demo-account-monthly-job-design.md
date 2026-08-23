# Demo account monthly job

Issue [#924](https://github.com/AndrewMcLachlan/MooBank/issues/924), part two. Follows
[the backfill spec](2026-08-22-demo-account-backfill-design.md), which brings the Demo family's
accounts up to date. This spec keeps them there.

The job runs on the first of each month and adds the month just ended. It does one month, the
previous one, and never looks further back: catching up is the backfill's job, and the backfill has
been amended to close the savings gap it previously deferred to here.

## Shape

A `TimerTrigger` job beside the existing four, delegating to a service, as they all do:

```csharp
[FunctionName("DemoData")]
public async Task Run([TimerTrigger("0 0 2 1 * *", RunOnStartup = RunOnStartup)] TimerInfo _, CancellationToken cancellationToken)
```

02:00 on the first of the month, an hour clear of `ExchangeRates` at midnight. `IDemoDataService`
lives in `src/MooBank/Services/` and is registered alongside `IRunRulesService`.

## The guard

The demo family is in production, beside real accounts, so the job writes only to instruments named
in configuration:

```json
"DemoData": {
  "CheckingAccountId": "...",
  "SavingsAccountId": "...",
  "MortgageAccountId": "...",
  "SuperAccountId": "...",
  "LoanAccountId": "...",
  "ElectricityAccountId": "...",
  "WaterAccountId": "..."
}
```

**Absence is the off switch.** No section, or an id left empty, means that piece is skipped and the
run logs why. There is no `Enabled` flag to get out of step with the ids. Development, staging and
any environment restored from a backup are inert by default, because none of them will have the
section unless somebody adds it.

Each id is resolved to an instrument before anything is written, and an id that does not resolve is
an error for that piece, not a silent no-op. A mistyped id must not read as "nothing to do".

## Idempotency: occupancy, not dedup

`Merchants._random` is a plain unseeded `Random`, so generating August twice produces two different
Augusts. The importer's duplicate detection would not recognise the second set, and the account
would quietly end up with double a month's spending.

So the check is **occupancy**: before writing to an account, ask whether it already holds any
transaction dated in the target month, and skip it if so. This is checked per account rather than
per run, so a run that got halfway through last time finishes the remaining accounts on the retry
without touching the ones already done.

Seeding the generator per month would also work, but it means threading a seed through static state
in `Merchants` for no benefit the occupancy check does not already give.

## Extracting the generator

`tools/MooBank.Tools.TransactionGenerator` is a self-contained console exe with no dependencies. It
splits into:

- `src/MooBank.DemoData/` — a class library holding `TransactionAccountGenerator`,
  `SavingsAccountGenerator`, `TransactionTemplates`, `Merchants`, `DescriptionBuilder`, `Transaction`.
- `tools/MooBank.Tools.TransactionGenerator` — `Program.cs` and the CSV writing, referencing the
  library. The tool keeps working exactly as it does now.

The library ships in the image, which is what makes the job in-image rather than an external script.

No behavioural change to the generators: they already take `(startingBalance, startDate, endDate)`,
which is a one-month window as readily as a ten-year one.

## What a run does

The target month is the whole previous calendar month, taken from the run date. Each piece below is
independently guarded and independently skippable, in its own `try`/`catch`, logged on failure, so
one broken piece does not cost the others their month.

**1. Checking.** `TransactionAccountGenerator` over the target month, seeded with the account's
current balance so spending behaviour stays plausible. Transactions are inserted directly; the
stored balance needs no maintenance, because `TransactionInstrument.Balance` reads from a view.

**2. Tagging.** `IRunRulesService.RunRules(checkingAccountId)` once the transactions are in. The
account has 164 rules with 164 rule-tags, which is how its history reached 99% coverage, and the
generator's merchants are the strings those rules match. Tagging new rows the same way the old ones
were tagged is what keeps the reports coherent.

This is why the job does not go through `IImportTransactionsService`, which would also apply rules:
that path needs a `User` for its audit call and an importer configured on the account, and a
fabricated user recorded as having performed an import is worse than the small amount of work
`RunRules` repeats.

RunRules reprocesses the whole account rather than only the new rows. On 9,300 transactions that is
a projection query plus the matched rows, once a month, which is not worth optimising. It does mean
a hand-applied tag that contradicts a rule would be overwritten — true of the demo family only in
principle, since nobody tags it by hand.

**3. Savings.** `SavingsAccountGenerator` for the month, given the transfers into savings that step
1 produced.

**4. Mortgage.** Derived from the checking `Mortgage` payments in the month: one amortising row
each, split into interest and principal, the interest split tagged `Mortgage Interest`. Derived
rather than scheduled, so the mortgage ledger cannot drift from the bank account.

**5. Super.** An employer contribution on each salary date in the month at the superannuation
guarantee rate in force, tagged `Employer Contribution`; plus an earnings row when the month ends a
quarter.

**6. Car loan.** One repayment row per checking `Car Loan` payment in the month. The loan matures
mid-2027, after which this piece has nothing to do and stops.

**7. Utilities.** A bill per checking electricity or water payment in the month, shaped so
`utilities.TotalCost` reproduces the amount paid, with meter readings continuing upward from the
last bill.

Pieces 4 through 7 are tagged explicitly at insert. Rules do the tagging on checking, where the
rules are; the derived accounts have none, and their tags are the `AccountTagPurpose` ones the
backfill created.

## Consequences of doing one month only

A run that fails, or an environment that was down on the first, leaves a permanent hole. Nothing
later fills it. This is the agreed behaviour, and the mitigation is that failures log at `Error`
and the gap is plainly visible in the demo itself, which is the thing being looked at.

## Duplicated arithmetic

The backfill amortises the mortgage and shapes the bills in T-SQL; this job does the same
arithmetic in C#. That duplication is deliberate: the backfill is hand-run once and is then
finished, and rewriting it to share code with a job that does not exist yet would hold up data that
is needed now.

What it costs is a continuity risk at the seam, so the job's first production run is verified
against the backfill's last rows before it is left alone: the first generated mortgage row must
continue the amortisation the script ended on, not restart it.

## Testing

- The month-occupancy guard: a second run over a month that already has transactions writes nothing.
- Missing configuration writes nothing, and an unresolvable id raises rather than passing quietly.
- The derivation arithmetic for mortgage, super and bills, which is where a wrong figure would
  survive unnoticed.
- Target-month calculation across a year boundary, so a run on 1 January fills the previous December.

## Verification after the first live run

| Check | Expectation |
|---|---|
| Checking rows for the month | present, and tagged at roughly the historical rate |
| First mortgage row | continues the backfill's amortisation |
| Mortgage interest + principal | equals the checking payment |
| Super balance | risen by the month's contributions and earnings |
| Bills for the month | `TotalCost` equals the checking payment |
| Re-running the job | writes nothing |
| Every non-demo account | untouched |
