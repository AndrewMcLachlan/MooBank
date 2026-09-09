# Mobile shell (MooBank) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make MooBank usable on a phone: convert every page to described actions, replace the transactions page header with a two-fact strip, and put the filter behind a bar that shows its own state.

**Architecture:** MooBank consumes the moo-app release that turns `Page`'s `actions` into `PageAction[]`, so Task 1 is a mechanical conversion across 23 files that must land in one commit — the app does not compile part-converted. Everything after that is additive: a mobile-only page header, a mobile-only filter bar over moo-ds's bottom `Drawer`, and three long-standing fixes.

**Tech Stack:** React 19, TypeScript 7, Vite 7, TanStack Router + Query, Vitest + Testing Library, moo-ds/moo-app.

**Spec:** `docs/superpowers/specs/2026-09-09-mobile-shell-design.md`

**Depends on:** `K:/Dev/Libraries/MooApp/docs/superpowers/plans/2026-09-09-mobile-shell.md` — all nine tasks merged and published. Take the published version from that PR before starting.

## Global Constraints

- **No utility CSS.** `d-flex`, `mb-3`, `text-center`, `col-*` and friends are not defined in this app and do nothing. Semantic class names, defined in a feature stylesheet under `src/css/`, imported into `App.css` with `layer(moobank)`.
- **No Bootstrap.** moo-ds is a custom component library. Never describe anything as Bootstrap.
- **camelCase constants.** Never `SCREAMING_SNAKE_CASE`.
- **Dates** through `src/utils/dateFns.ts`; **amounts** through the `Amount` component or `src/utils/currency.ts`. No `toLocaleDateString`, no hand-rolled `Intl.NumberFormat`.
- **`strictNullChecks: false` and `noImplicitAny: false` are deliberate.** Do not "fix" them.
- **TypeScript 7 stays.** Never downgrade the compiler to satisfy tooling. Check `npx tsc --version` is 7.x after any dependency change.
- Comments explain why a thing *is*, never what changed. No "was X, now Y".
- Run from `src/MooBank.Web.App`: `npm run build`, `npm run lint`, `npm test`.

---

### Task 1: Bump moo-app and convert every actions call site

The app will not compile between the bump and the last conversion, so this is one commit. Twenty-three files pass `actions`; convert them all.

**Files:**
- Modify: `src/MooBank.Web.App/package.json`, `package-lock.json`
- Modify: all 23 files listed by the grep in Step 1
- Modify: `src/MooBank.Web.App/src/components/InstrumentPage.tsx`, `src/routes/bills/-components/BillsPage.tsx`, `src/routes/budget/-components/BudgetPage.tsx`, `src/routes/settings/-components/SettingsPage.tsx`, `src/routes/planning/-components/ForecastPage.tsx`, `src/routes/planning/-retirement-components/RetirementPage.tsx` (page wrappers that forward `actions`)

**Interfaces:**
- Consumes: `PageAction`, `PageProps.customActions` from moo-app
- Produces: an app that compiles against the new moo-app

- [ ] **Step 1: Bump and find the damage**

```bash
cd src/MooBank.Web.App
npm install @andrewmclachlan/moo-app@<published> @andrewmclachlan/moo-ds@<published>
npx tsc --version    # must be 7.x
grep -rn 'actions=' src --include=*.tsx
```

Expected: `npm run build` fails with type errors at every `actions` site.

- [ ] **Step 2: Retype the wrappers**

Six components forward `actions` through their own props. Change each `actions?: React.ReactNode[]` to `actions?: PageAction[]` and add `customActions?: React.ReactNode[]`, forwarding both. Example, `src/components/InstrumentPage.tsx`:

```tsx
import type { PageAction } from "@andrewmclachlan/moo-app";

export interface InstrumentPageProps {
    actions?: PageAction[];
    customActions?: React.ReactNode[];
}
```

```tsx
<Page title={...} actions={props.actions} customActions={props.customActions} navItems={...} breadcrumbs={...}>
```

- [ ] **Step 3: Convert the call sites**

A command becomes an `onClick` action; a link becomes a `to` action; a switch stays a switch. Three worked examples covering all three shapes — apply the same treatment everywhere.

`src/routes/accounts/-transactions/Transactions.tsx`:

```tsx
    let actions: PageAction[] = [
        { id: "show-net-amount", label: "Show Net Amount", checked: showNet, onClick: () => setShowNet(!showNet) },
        { id: "compact-mode", label: "Compact", checked: compactMode, onClick: () => setCompactMode(!compactMode) },
    ];

    switch (account.controller) {
        case "Manual":
        case "Virtual":
            actions = [...actions, { id: "add", label: "Add", icon: <Icon icon="plus" />, group: "write", onClick: () => setShow(true) }];
            break;
        case "Import":
            actions = [...actions, { id: "import", label: "Import", icon: <Icon icon="upload" />, group: "write", onClick: () => setShowImport(true) }];
            break;
        default:
            break;
    }
```

`src/routes/settings/families/index.tsx` — a link keeps being a link:

```tsx
<SettingsPage
    title="Families"
    breadcrumbs={[{ text: "Families", route: "/settings/families" }]}
    actions={[{ id: "add-family", label: "Add Family", icon: <Icon icon="plus" />, group: "write", to: "/settings/families/add" }]}
>
```

`src/routes/accounts/$id/rules.tsx` — a plain command:

```tsx
actions={[{ id: "run-rules", label: "Run Rules", icon: <Icon icon="check" />, onClick: runRules }]}
```

Anything that is genuinely a control rather than a command or a link goes to `customActions` unchanged.

- [ ] **Step 4: Verify**

```bash
npm run build
npm run lint
npm test
```

Expected: all pass. Then `npm start` and check a few pages at 1280px — the actions row should look exactly as it did.

- [ ] **Step 5: Commit**

```bash
git add src/MooBank.Web.App
git commit -m "refactor(web): describe page actions

Takes the moo-app release that replaces Page's ReactNode[] actions with
PageAction[], so a mobile header can render them as a menu. Links stay
links rather than becoming navigate() handlers.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01UMhsYDPBrTkeKYZTvUzBDg"
```

---

### Task 2: The transactions mobile header

Two facts. The account name is already the app bar title and the period is already the filter bar's own control, so the card was repeating both.

**Files:**
- Create: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionsMobileHeader.tsx`
- Create: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/__tests__/TransactionsMobileHeader.test.tsx`
- Modify: `src/MooBank.Web.App/src/css/transactions/transactionsHeader.css`

**Interfaces:**
- Consumes: `useAccount`, `useTransactionPeriodStats(instrumentId): { income, expenses, net, total }`, `Amount`
- Produces: `<TransactionsMobileHeader />`

- [ ] **Step 1: Write the failing test**

`src/routes/accounts/-transactions/components/__tests__/TransactionsMobileHeader.test.tsx`. Read a neighbouring test first — `src/routes/accounts/-transactions/components/AddTransaction.test.tsx` — and reuse its provider harness and mocking style rather than inventing another.

```tsx
import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { TransactionsMobileHeader } from "../TransactionsMobileHeader";

vi.mock("components", async (importOriginal) => ({
    ...(await importOriginal<typeof import("components")>()),
    useAccount: () => ({ id: "1", name: "Joint Savings", currency: "AUD", currentBalance: 12480.22 }),
}));

vi.mock("../../hooks/useTransactionPeriodStats", () => ({
    useTransactionPeriodStats: () => ({ income: 4120, expenses: 2844, net: 1276, total: 41 }),
}));

describe("TransactionsMobileHeader", () => {
    it("shows the balance under a Balance label", () => {
        render(<TransactionsMobileHeader />);
        expect(screen.getByText("Balance")).toBeInTheDocument();
        expect(screen.getByText("$12,480.22")).toBeInTheDocument();
    });

    it("shows the period net, signed", () => {
        render(<TransactionsMobileHeader />);
        expect(screen.getByText("Net")).toBeInTheDocument();
        expect(screen.getByText("+$1,276.00")).toBeInTheDocument();
    });

    it("does not repeat the account name, the period or the transaction count", () => {
        render(<TransactionsMobileHeader />);
        expect(screen.queryByText(/Joint Savings/)).not.toBeInTheDocument();
        expect(screen.queryByText(/41/)).not.toBeInTheDocument();
    });
});
```

A negative net is the case worth proving, since the sign and the colour are the whole point of the figure. Add a second `describe` with its own mock:

```tsx
describe("TransactionsMobileHeader, negative net", () => {
    beforeEach(() => {
        vi.doMock("../../hooks/useTransactionPeriodStats", () => ({
            useTransactionPeriodStats: () => ({ income: 100, expenses: 512, net: -412, total: 9 }),
        }));
    });

    it("shows a negative net signed and in the expense colour", async () => {
        const { TransactionsMobileHeader: Header } = await import("../TransactionsMobileHeader");
        render(<Header />);
        const net = screen.getByText("-$412.00");
        expect(net).toBeInTheDocument();
        expect(net.closest(".negative")).not.toBeNull();
    });
});
```

Check the exact rendered string and class against `src/components/Amount.tsx` before asserting — `Amount` owns the minus glyph and the `negative` class, and this test should follow it rather than restate it.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- TransactionsMobileHeader`
Expected: FAIL — cannot resolve `../TransactionsMobileHeader`.

- [ ] **Step 3: Write the implementation**

```tsx
import React from "react";

import { Amount, useAccount } from "components";
import type { LogicalAccount } from "api/types.gen";

import { useTransactionPeriodStats } from "../hooks/useTransactionPeriodStats";

/**
 * The transactions page header on a phone. The account name is the app bar
 * title and the period is the filter bar's own control, so neither is repeated
 * here; what is left is where the account stands and which way the selected
 * period moved it.
 */
export const TransactionsMobileHeader: React.FC = () => {

    const account = useAccount();
    const stats = useTransactionPeriodStats(account?.id ?? "");

    if (!account) return null;

    const balance = (account as LogicalAccount).currentBalance ?? 0;

    return (
        <section className="tx-mobile-header">
            <div className="tx-mobile-figure">
                <div className="lbl">Balance</div>
                <div className="val balance"><Amount amount={balance} currencyCode={account.currency} minus /></div>
            </div>
            <div className="tx-mobile-figure net">
                <div className="lbl">Net</div>
                <div className="val"><Amount amount={stats.net} currencyCode={account.currency} plus minus positiveColour negativeColour zeroShowsAs="neutral" /></div>
            </div>
        </section>
    );
};

TransactionsMobileHeader.displayName = "TransactionsMobileHeader";
```

Append to `src/css/transactions/transactionsHeader.css`:

```css
/* ===========================================
   Mobile transactions header
   =========================================== */
.tx-mobile-header {
    position: relative;
    display: flex;
    align-items: flex-end;
    justify-content: space-between;
    gap: 0.75rem;
    padding: 0.5rem 0 0.55rem 0.75rem;
    border-bottom: 1px solid var(--border-colour);
}

.tx-mobile-header::before {
    content: "";
    position: absolute;
    left: 0;
    top: 0.4rem;
    bottom: 0.45rem;
    width: 3px;
    background: var(--primary);
    border-radius: 2px;
}

.tx-mobile-header .lbl {
    font-size: 0.65rem;
    font-weight: 700;
    letter-spacing: 0.8px;
    text-transform: uppercase;
    color: var(--body-colour-dim);
}

.tx-mobile-header .val {
    font-size: 0.95rem;
    font-weight: 600;
    font-variant-numeric: tabular-nums;
    letter-spacing: -0.3px;
    line-height: 1.05;
}

.tx-mobile-header .val.balance {
    font-size: 1.6rem;
    letter-spacing: -1px;
    color: var(--body-colour-bold);
}

.tx-mobile-header .net {
    text-align: right;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- TransactionsMobileHeader`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionsMobileHeader.tsx src/MooBank.Web.App/src/routes/accounts/-transactions/components/__tests__/TransactionsMobileHeader.test.tsx src/MooBank.Web.App/src/css/transactions/transactionsHeader.css
git commit -m "feat(web): add the transactions mobile header

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01UMhsYDPBrTkeKYZTvUzBDg"
```

---

### Task 3: The transactions filter bar and sheet

**Files:**
- Create: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionsFilterBar.tsx`
- Create: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/__tests__/TransactionsFilterBar.test.tsx`
- Create: `src/MooBank.Web.App/src/css/transactions/transactionsFilterBar.css`
- Modify: `src/MooBank.Web.App/src/App.css` (add the `@import`)

**Interfaces:**
- Consumes: `FilterBar`, `FilterChip`, `Drawer`, `Input` from moo-ds; `useFilterPanel()` (existing), `DateRangeSelector`, `TagSelector`
- Produces: `<TransactionsFilterBar />`

- [ ] **Step 1: Write the failing test**

`__tests__/TransactionsFilterBar.test.tsx`:

```tsx
import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { TransactionsFilterBar } from "../TransactionsFilterBar";

const setFilterTags = vi.fn();
const setFilterType = vi.fn();

vi.mock("../../hooks/useFilterPanel", () => ({
    useFilterPanel: () => ({
        filterDescription: "", filterTagged: false, filterNetZero: false,
        filterTags: [1], filterType: "Expense",
        setFilterDescription: vi.fn(), setFilterTagged: vi.fn(), setFilterNetZero: vi.fn(),
        setFilterTags, setFilterType, setPeriod: vi.fn(),
    }),
}));

describe("TransactionsFilterBar", () => {
    it("counts the active filters, excluding the period", () => {
        render(<TransactionsFilterBar />);
        expect(screen.getByText("2")).toBeInTheDocument();
    });

    it("renders a chip per active filter", () => {
        render(<TransactionsFilterBar />);
        expect(screen.getByText("Groceries")).toBeInTheDocument();
        expect(screen.getByText("Expense")).toBeInTheDocument();
    });

    it("clears only that filter when a chip is dismissed", () => {
        render(<TransactionsFilterBar />);
        fireEvent.click(screen.getByRole("button", { name: "Remove Expense" }));
        expect(setFilterType).toHaveBeenCalledWith("");
        expect(setFilterTags).not.toHaveBeenCalled();
    });

    it("opens the sheet from the Filters button", () => {
        const { container } = render(<TransactionsFilterBar />);
        expect(container.querySelector(".offcanvas.show")).not.toBeInTheDocument();
        fireEvent.click(screen.getByRole("button", { name: /filters/i }));
        expect(container.querySelector(".offcanvas-bottom.show")).toBeInTheDocument();
    });
});
```

The tag chip resolves id `1` to a name, so the tags query needs mocking too. Add above the `describe`:

```tsx
vi.mock("@tanstack/react-query", async (importOriginal) => ({
    ...(await importOriginal<typeof import("@tanstack/react-query")>()),
    useQuery: () => ({ data: [{ id: 1, name: "Groceries" }] }),
}));
```

That is blunt — it stubs every `useQuery` in the tree — and it is adequate here because this component issues exactly one. If the component later grows a second query, switch to a `QueryClientProvider` with seeded data instead.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- TransactionsFilterBar`
Expected: FAIL — cannot resolve `../TransactionsFilterBar`.

- [ ] **Step 3: Write the implementation**

```tsx
import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Drawer, FilterBar, FilterChip, Input } from "@andrewmclachlan/moo-ds";

import { getTagsOptions } from "api/@tanstack/react-query.gen";
import { TagSelector } from "components";
import { DateRangeSelector } from "components/DateRangeSelector";
import type { transactionTypeFilter } from "models/transactions";

import { useFilterPanel } from "../hooks/useFilterPanel";

export const TransactionsFilterBar: React.FC = () => {

    const [showSheet, setShowSheet] = useState(false);
    const { data: tags } = useQuery(getTagsOptions());

    const {
        filterDescription, filterTagged, filterNetZero, filterTags, filterType,
        setFilterDescription, setFilterTagged, setFilterNetZero, setFilterTags, setFilterType, setPeriod,
    } = useFilterPanel();

    const tagName = (id: number) => tags?.find(t => t.id === id)?.name ?? String(id);

    const clearAll = () => {
        setFilterDescription("");
        setFilterTags([]);
        setFilterType("" as transactionTypeFilter);
        setFilterTagged(false);
        setFilterNetZero(false);
    };

    const activeCount =
        (filterDescription ? 1 : 0) +
        filterTags.length +
        (filterType ? 1 : 0) +
        (filterTagged ? 1 : 0) +
        (filterNetZero ? 1 : 0);

    return (
        <>
            <FilterBar
                className="tx-filter-bar"
                primary={<DateRangeSelector onChange={setPeriod} />}
                activeCount={activeCount}
                onOpenFilters={() => setShowSheet(true)}
                onClear={activeCount > 0 ? clearAll : undefined}
            >
                {filterDescription && (
                    <FilterChip key="description" onRemove={() => setFilterDescription("")}>{filterDescription}</FilterChip>
                )}
                {filterTags.map(id => (
                    <FilterChip key={`tag-${id}`} onRemove={() => setFilterTags(filterTags.filter(t => t !== id))}>{tagName(id)}</FilterChip>
                ))}
                {filterType && (
                    <FilterChip key="type" onRemove={() => setFilterType("" as transactionTypeFilter)}>{filterType}</FilterChip>
                )}
                {filterTagged && (
                    <FilterChip key="untagged" onRemove={() => setFilterTagged(false)}>Untagged</FilterChip>
                )}
                {filterNetZero && (
                    <FilterChip key="offset" onRemove={() => setFilterNetZero(false)}>Excluding offset</FilterChip>
                )}
            </FilterBar>

            <Drawer show={showSheet} onHide={() => setShowSheet(false)} placement="bottom" className="tx-filter-sheet">
                <Drawer.Header closeButton><h2>Filters</h2></Drawer.Header>
                <Drawer.Body>
                    <Input id="filter-desc" type="search" label="Description" value={filterDescription} onChange={(e) => setFilterDescription(e.currentTarget.value)} placeholder="Contains..." />
                    <TagSelector id="filter-tags" label="Tags" onChange={setFilterTags} multiSelect value={filterTags} />
                    <Input.Select id="filter-type" label="Type" value={filterType} onChange={(e) => setFilterType(e.currentTarget.value as transactionTypeFilter)}>
                        <option value="">All</option>
                        <option value="Income">Income</option>
                        <option value="Expense">Expense</option>
                    </Input.Select>
                    <Input.Switch id="filter-tagged" label="Only untagged" checked={filterTagged} onChange={(e) => setFilterTagged(e.currentTarget.checked)} />
                    <Input.Switch id="filter-netzero" label="Exclude fully offset" checked={filterNetZero} onChange={(e) => setFilterNetZero(e.currentTarget.checked)} />
                </Drawer.Body>
            </Drawer>
        </>
    );
};

TransactionsFilterBar.displayName = "TransactionsFilterBar";
```

`src/css/transactions/transactionsFilterBar.css`:

```css
.tx-filter-sheet .offcanvas-body {
    display: flex;
    flex-direction: column;
    gap: 0.85rem;
    padding-bottom: 1rem;
}

/* A finger, not a pointer. */
.tx-filter-sheet .offcanvas-body :is(input, select, button) {
    min-height: 2.75rem;
}
```

Add to `src/App.css`, beside the other transactions imports:

```css
@import "css/transactions/transactionsFilterBar" layer(moobank);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- TransactionsFilterBar`
Expected: PASS, 4 tests.

- [ ] **Step 5: Commit**

```bash
git add src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionsFilterBar.tsx src/MooBank.Web.App/src/routes/accounts/-transactions/components/__tests__/TransactionsFilterBar.test.tsx src/MooBank.Web.App/src/css/transactions/transactionsFilterBar.css src/MooBank.Web.App/src/App.css
git commit -m "feat(web): add the transactions filter bar and sheet

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01UMhsYDPBrTkeKYZTvUzBDg"
```

---

### Task 4: Compose the page

Mobile is its own composition — not compact mode, not the normal view.

**Files:**
- Modify: `src/MooBank.Web.App/src/routes/accounts/-transactions/Transactions.tsx`
- Modify: `src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionList.tsx`

**Interfaces:**
- Consumes: `useIsAtLeast` from moo-ds; Tasks 2 and 3
- Produces: the finished transactions page

- [ ] **Step 1: Branch the page on the content breakpoint**

In `Transactions.tsx`, replace `useIsDesktop` and the three-way body:

```tsx
    const isPhone = !useIsAtLeast("md");
```

Drop the Compact action from `actions` when `isPhone` — mobile is neither mode, so the switch has nothing to switch. Then:

```tsx
            {isPhone ? (
                <>
                    <TransactionsMobileHeader />
                    <TransactionsFilterBar />
                </>
            ) : compactMode ? (
                <>
                    <TransactionsCompactWidgets />
                    <MiniFilterPanel />
                </>
            ) : (
                <SectionRow>
                    <Col xxl={5} xl={12} lg={12} md={12} sm={12}>
                        <TransactionsAccountCard />
                    </Col>
                    <Col xxl={7} xl={12} lg={12} md={12} sm={12}>
                        <FilterPanel />
                    </Col>
                </SectionRow>
            )}
            <TransactionList compact={isPhone} />
```

- [ ] **Step 2: Remove the redundant visibility classes**

In `TransactionList.tsx`:

```tsx
    const className = compact ? "transactions-mobile" : "transactions";
```

Only one table is ever rendered and the caller has already chosen which, so `d-none d-md-table` could only hide the table that is meant to be there.

- [ ] **Step 3: Verify**

```bash
npm run build
npm test
```

Expected: PASS. Then `npm start` and check the transactions page at 1280px (unchanged), 900px (drawer nav, desktop content) and 390px (mobile header, filter bar, two-column table).

- [ ] **Step 4: Commit**

```bash
git add src/MooBank.Web.App/src/routes/accounts/-transactions/Transactions.tsx src/MooBank.Web.App/src/routes/accounts/-transactions/components/TransactionList.tsx
git commit -m "feat(web): compose the transactions page for a phone

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01UMhsYDPBrTkeKYZTvUzBDg"
```

---

### Task 5: Three fixes

Independent of each other and of everything above; they only need doing once.

**Files:**
- Modify: `src/MooBank.Web.App/src/App.css:190`
- Modify: `src/MooBank.Web.App/src/css/filterPanel.css`
- Delete: `src/MooBank.Web.App/src/hooks/useMediaQuery.ts`, `src/MooBank.Web.App/src/hooks/useMediaQuery.test.ts`
- Modify: `src/MooBank.Web.App/src/hooks/index.ts`

- [ ] **Step 1: Delete the rule that stopped matching**

Remove from `src/App.css`:

```css
    @media screen and (max-width: 767px) {
        .container-fluid.second-header>.actions {
            display: none;
        }
    }
```

moo-ds dropped the container class — its own comment says *"Was .container/.container-fluid"* — so this has matched nothing since that release. Confirm before deleting:

```bash
grep -c "container-fluid" src/MooBank.Web.App/node_modules/@andrewmclachlan/moo-ds/dist/index.css
grep -n "second-header" src/MooBank.Web.App/node_modules/@andrewmclachlan/moo-ds/dist/index.css
```

Expected: `second-header` appears with no `container-fluid` alongside it.

- [ ] **Step 2: Give the description search a basis**

In `src/css/filterPanel.css`:

```css
    #filter-desc {
        flex: 1 1 12rem;
    }
```

Basis `0` let it collapse to nothing once the row wrapped, while `#filter-tags` (175px) and `#filter-type` (125px) held their width. This still matters after the sheet lands, because `MiniFilterPanel` remains the desktop compact filter.

- [ ] **Step 3: Remove the local media-query hook**

```bash
grep -rn "useIsDesktop\|useMediaQuery" src/MooBank.Web.App/src
```

Expected after Task 4: only the hook file, its test, and the `src/hooks/index.ts` barrel line. Delete both files and the `export * from "./useMediaQuery";` line.

- [ ] **Step 4: Verify**

```bash
npm run build
npm run lint
npm test
```

Expected: all pass. Then `npm start` and check the compact desktop filter at 1100px — the description field should hold a usable width when the row wraps.

- [ ] **Step 5: Commit and open the PR**

```bash
git add src/MooBank.Web.App/src/App.css src/MooBank.Web.App/src/css/filterPanel.css src/MooBank.Web.App/src/hooks
git commit -m "fix(web): drop a dead mobile rule and a collapsing filter input

The .container-fluid.second-header rule stopped matching when moo-ds
removed the container class, so the mobile action-hiding it intended
never happened. #filter-desc had flex-basis 0 with no minimum, so it
collapsed to nothing whenever the mini filter row wrapped.

Co-Authored-By: Claude Opus 5 (1M context) <noreply@anthropic.com>
Claude-Session: https://claude.ai/code/session_01UMhsYDPBrTkeKYZTvUzBDg"
```

---

## Notes for the executor

- **Branch:** create `feature/mobile-shell` from `main` before Task 1. The current working branch is `feature/retirement-return-rates` and is unrelated.
- **Task 1 is one commit** and a large one. That is deliberate: a partial conversion does not compile.
- **Do not touch `src/api/*.gen.ts` or `routeTree.gen.ts`.** They are generated.
- **Check the browser at three widths** before the PR: 1280px, 900px and 390px. The header overlap that started this is a layout bug no unit test would have caught, and it needs looking at on a light theme and a dark one — `--header-bg` and `--breadcrumb-bg` diverge on `.light.red` and are identical on dark.
- **Left open by the spec, deliberately not in this plan:** whether the two-column row should show its tag, and the other pages (Accounts, Budget, and the report pages, where a chart at 390pt is its own problem).
