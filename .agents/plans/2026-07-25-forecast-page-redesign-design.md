# Design — Forecast page redesign

**Date:** 2026-07-25
**Status:** Approved design, pending spec review → implementation plan.

## Goal

Bring the Forecast page (`routes/forecast/`) up to the app's current design language. The page was
vibe-coded and has drifted: it leads with settings instead of the answer, reinvents stat/label
styling that moo-ds already provides, hard-codes chart colours the rest of the app themes, and uses
an off-pattern outline button. The page's primary job is **reading the outlook** ("am I on track?"),
so the redesign leads with the projection + risk figures and demotes the editable inputs.

No API/backend changes. No OpenAPI/client regeneration. Frontend and CSS only.

## Decisions (from brainstorming)

- **Direction:** chart-led *Outlook* with a KPI row (not a verdict banner or two-column rail).
- **Primary use:** read the outlook → answer-first ordering.
- **Edit pattern:** modal, opened from a page-header action (not inline edit, not a drawer).
- **Header:** the plan name is the page title.
- **Planned tables:** side-by-side.
- **Edit button icon:** `Sliders`.

Mockup (reference only, not the implementation): the interactive dark-theme mockup approved during
brainstorming.

## New page order

`Outlook` → `Forecast Settings` (read-only summary) → `Planned Income` / `Planned Expenses`
(side-by-side). Today the order is Settings → Summary → Chart → Planned tables.

## Page header

`ForecastPage` currently hard-codes `title="Forecast"`. Change it to take the loaded plan and set
`title={plan.name}` with the plan name as the breadcrumb leaf (`Home / Forecast / <plan name>`).
Because the name is now the header, the **"Plan Name" field is removed from the Settings summary**
(still editable in the modal).

The **Edit Settings** action moves to the page-level actions slot (where `Transactions.tsx` /
`accounts/index.tsx` put their primary actions), rendered as the app's standard on-brand control:

```tsx
<IconButton badge variant="primary" icon={Sliders} onClick={() => setEditOpen(true)}>Edit Settings</IconButton>
```

This replaces the current `<Button variant="outline-primary" size="sm">` inside the Settings section
header — that variant is reserved for Close/secondary modal actions, so it read as off-brand and was
low-contrast on the dark ground.

## Outlook section (new `ForecastOutlook.tsx`)

One `Section header="Outlook"` that absorbs today's `ForecastSummaryPanel` **and** the chart:

- **KPI risk band** — three figures, left-aligned, as one stat row using `Amount` for money:
  *Lowest Balance* (+ "in <month>"), *Months Below Zero* (+ "never runs negative" / "needs
  attention"), *Required Monthly Uplift* (+ "no uplift required" / "to avoid negative balance").
  Each value takes a `negative` treatment **only when it signals risk** (lowest balance `< 0`,
  months below zero `> 0`, required uplift `> 0`) — a cheap "am I OK?" read carried over from the
  current `summary-value.negative` logic.
- **Health pill** in the section header — `On track` (green) when `monthsBelowZero === 0 &&
  requiredMonthlyUplift <= 0`, otherwise `Needs attention` (amber). Derived from the same summary
  fields; no new data.
- **Chart** — the balance projection, switched to **theme-aware `useChartColours()`** for the grid
  and the Actual series (matching the dashboard `ForecastWidget`, which already does this). The
  standalone `ForecastChart` currently hard-codes `rgb(53,162,235)` / `rgb(34,197,94)` and is the
  *only* forecast chart not themed.
- **Secondary totals** — *Projected income $X · projected outgoings $Y* as a quiet caption below the
  chart (was two large `summary-card`s).
- **Regression fallback** — the "correlation too weak (R² …)" note as a subtle chart caption (was its
  own full-width card row).
- **Loading** — the `resultLoading` spinner lives inside this section, removing the ad-hoc `<div>`
  wrapper in `index.tsx`.

### Shared chart config

Extract the Chart.js data/options builder shared by `ForecastOutlook` (full page) and
`routes/-dashboard/Forecast.tsx` (widget) into one helper (e.g. `forecast/-utils/forecastChart.ts`)
so the two charts can't drift again. The widget keeps its ±6-month windowing; the page shows the full
range. This is the fix for the theming inconsistency, applied once.

## Forecast Settings (read-only summary + modal)

`ForecastSettings.tsx` splits into two responsibilities:

1. **Read-only summary** (stays in `ForecastSettings.tsx`) — a `Section header="Forecast Settings"`
   rendering `KeyValue`-style pairs (Period, Monthly Income, Monthly Expenses + "income-correlated,
   flat average" sublabel, Accounts) instead of the bespoke `.settings-item` uppercase blocks. No
   Plan Name (it's the header). The regression `OverlayTrigger`/`Popover` on Monthly Expenses is
   preserved.
2. **Edit modal** (new `ForecastSettingsModal.tsx`) — a moo-ds `Modal` containing the existing
   react-hook-form form from PR #919, transplanted almost verbatim: `useForm({ values: toFormValues
   (plan), resetOptions: { keepDirtyValues: true } })`, the `useWatch` fields, `handleSave` →
   `useUpdateForecastPlan`, the account toggle logic. Footer follows the app modal idiom:
   `Button variant="outline-primary"` (Close) + `Button type="submit" variant="primary"` (Save). The
   inline `isEditing` toggle becomes the modal's open state, lifted to the page so the header action
   can open it.

Form behaviour, validation, and the update mutation are unchanged — only the container (inline
`SectionForm` → `Modal`) and trigger location change.

## Planned Income / Expenses

Unchanged in substance — already house-style `SectionTable` with `EditColumn` inline editing,
`Amount`, `NewPlannedItem` add-row, and totals. Only change: rendered **side-by-side** (two columns)
rather than stacked full-width, stacking on narrow viewports. Full columns (Name, Amount, Start, End,
Frequency, Notes, action) are kept; they compress on the shared width and stack below the layout
breakpoint.

## CSS (`css/forecast.css`)

- **Delete** `.forecast-summary` / `.summary-card` / `.settings-item` blocks.
- **Add** semantic classes for the KPI band, the health pill, the settings `KeyValue` summary, and
  the two-column planned-tables layout (CSS grid, stacks at the layout breakpoint).
- **Keep** `.regression-hint` / `.regression-popover`, `.forecast-widget-chart` (dashboard widget),
  and `.new-planned-item` — all still used.

## Components summary

| File | Change |
|------|--------|
| `-components/ForecastOutlook.tsx` | **New** — KPI band + health pill + themed chart + captions + spinner |
| `-components/ForecastChart.tsx` | Simplified to a themed presentational chart (no own `Section`); consumes shared config |
| `-components/ForecastSummaryPanel.tsx` | **Deleted** — absorbed into `ForecastOutlook` |
| `-components/ForecastSettings.tsx` | Reduced to the read-only `KeyValue` summary |
| `-components/ForecastSettingsModal.tsx` | **New** — the RHF form from #919 in a `Modal` |
| `-components/ForecastPage.tsx` | Takes the plan; `title={plan.name}`, breadcrumb leaf, Edit Settings action |
| `-utils/forecastChart.ts` | **New** — shared Chart.js data/options for page + dashboard widget |
| `index.tsx` | Reordered; lifts the modal open state; wires the header action |
| `routes/-dashboard/Forecast.tsx` | Consumes the shared chart config (no behaviour change) |
| `css/forecast.css` | Delete bespoke stat/settings CSS; add KPI band / pill / summary / two-col table styles |

## Testing

Light Vitest (per the "test by value" harness):

- KPI risk-colour logic: risk fields render `negative` when over threshold, neutral otherwise; the
  health pill flips `On track` ↔ `Needs attention` on `monthsBelowZero` / `requiredMonthlyUplift`.
- Settings modal: header action opens the modal; Save calls the update mutation; Close dismisses
  without mutating.

## Out of scope

Multi-plan switching (the page still uses `plans[0]`), any change to the forecast calculation, and
any backend/DTO change.
