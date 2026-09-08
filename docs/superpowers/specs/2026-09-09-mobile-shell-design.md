# Mobile shell

MooBank is unusable on a phone. The header overlaps itself, the page actions overflow off-screen,
and the transactions page spends two screens on chrome before showing a transaction.

Almost none of that is MooBank's. `moo-ds/src/css/layout/_header.css` contains no media queries at
all — the app chrome is styled once, for a desktop — and `moo-app/src/layout/Mobile/Header.tsx` is
the desktop header with a different visibility class. So the work is mostly upstream, and this spec
covers both repositories. It is kept here because MooBank is where the problem was found and where
the result is judged; the moo-app PR should link back to it.

## What a phone is for

Every decision below follows from one premise: **on a phone you read your finances, you do not
administer them.** You check a balance, see where the month went, look for a transaction you half
remember. You do not upload a CSV export or create a bank account.

That gives a rule for placing anything: permanent screen space is earned by what you came for.
Everything else is one tap away, and nothing is removed.

| Action | Desktop | Mobile |
|---|---|---|
| Change period | Filter panel field | Its own control on the filter bar |
| Tags, type, description | Filter panel fields | Filter sheet, summarised as chips |
| Show net amount | Header switch | Overflow menu, checked item |
| Compact | Header switch | Not shown — see *Mobile is not compact mode* |
| Import transactions | Header button | Overflow menu |
| Add transaction | Header button | Overflow menu |
| Create account | Page button | Overflow menu |

No floating action button. A FAB claims a corner permanently for something done a few times a year.

---

# Part one — moo-app and moo-ds

## The mobile header

One bar, 56px: the drawer toggle, the page title, an overflow button.

```tsx
<header className="mobile-header d-lg-none">
    <MenuToggle onClick={() => setShowSidebar(true)} />
    <h1 className="page-title">{currentCrumb}</h1>
    {customActions}
    <ActionMenu actions={actions} />
</header>
```

The title is the **last breadcrumb**, not the trail. Going up a level is what the back gesture and
the drawer are for, and a trail is the one thing guaranteed not to fit: "Home / Accounts / Joint
Savings" wrapped to three lines in the reported bug. It ellipsises rather than wraps, so the bar can
never grow past its own height — the failure mode that produced the overlap.

The first band goes entirely on mobile. It carries a logo, a search box that the mobile header
already renders empty, and a user menu that belongs in the drawer.

## Three defects that force this

**The header is sized for a search box mobile never renders.** `_header.css`:

```css
&.first-header {
    grid-template-columns: 1.5fr minmax(240px, 1fr) 1.5fr;
}
```

`Mobile/Header.tsx` renders `<div className="search">` with nothing inside, but the 240px minimum
still applies, forcing the header past 600px. On a 390pt phone the row overflows and the last action
is clipped off-screen.

**160px of bands in an 80px track.** `_layout.css` gives the header a hard `80px` grid row, and both
bands then ask for `var(--header-height)` — 80px under `moo-default`, which is what MooBank gets
since its `Layout.tsx` passes no `size`. Two 80px bands in an 80px track is why the breadcrumb runs
across the logo. The single bar fixes this by construction, but the grid row should become
`min-content` so the header is never again shorter than what it contains.

**`--header-height` is doing two jobs.** It sizes the header *and* each band. Once mobile has one
band and desktop has two, that has to split: the track takes its height from content, and each band
sizes itself.

## `actions` becomes described

`Page` takes `actions?: ReactNode[]` and both headers drop the nodes into a flex row. The header
cannot re-render an opaque node as a menu item, so an overflow menu is impossible without the header
knowing what an action *is*.

```ts
export interface PageAction {
    label: string;
    icon?: React.ReactNode;
    onClick: () => void;
    /** Present makes it a toggle; the menu shows a tick and the desktop header a switch. */
    checked?: boolean;
    /** "read" sorts above the separator, "write" below. Defaults to "read". */
    group?: "read" | "write";
    disabled?: boolean;
    variant?: "primary" | "secondary";
}

export interface PageProps {
    actions?: PageAction[];
    /** Escape hatch for controls that are not a label and a handler. */
    customActions?: React.ReactNode[];
}
```

This is a **breaking change**: `actions` no longer accepts nodes. That is deliberate. An additive
union would leave a silent second-class path where a bare node works on desktop and vanishes on
mobile, which is exactly the kind of failure that goes unnoticed for a release.

`customActions` is the pressure valve, and it is a different prop precisely so it reads as an
exception. Anything that is genuinely a control rather than a command — a search field, a segmented
selector — goes there. It renders inline in the actions row on desktop and inline in the bar on
mobile, before the overflow button. moo-app makes no promise that it fits a phone; that is the
caller's problem, and the narrowness of the prop is the warning.

`LayoutProvider` changes its `actions` state to `PageAction[]` and gains `customActions`. The two
consumers of `useLayout().actions` are the two headers.

## The overflow menu

moo-ds has no menu component. `moo-app/src/layout/UserMenu.tsx` builds one from `OverlayTrigger` and
`Popover`, and that arrangement should be promoted to a moo-ds `Menu` with `Menu.Item` and
`Menu.Divider`, then used by both `UserMenu` and the new `ActionMenu`. Promoting it is what stops
the third menu in the codebase being hand-rolled again.

`ActionMenu` groups by `PageAction.group` with a divider between, so reading toggles never sit
adjacent to a destructive or writing action and cannot be hit by mistake.

## Two breakpoints, both named

"Mobile" currently means four different widths: 992px in the layout (`d-lg-*`), 768px in MooBank's
`TransactionList` (`d-md-*`), 768px again in `useIsDesktop`, and 576px in the account list
(`d-sm-table-cell`). Nothing declares any of them, and nothing relates them to each other.

The fix is not one breakpoint. Two are genuinely needed, because chrome and content stop fitting at
different widths:

- **`lg` (992px) — chrome.** Where the sidebar becomes a drawer. The mobile header follows this.
- **`md` (768px) — content.** Where a page has to restructure. The phone page header, the filter
  sheet and the two-column table follow this.

Between them, a tablet gets drawer navigation with desktop content, which is correct: at 900px there
is ample room for a full card and inline filters. Today's arrangement is roughly this by accident,
which is why the two numbers were never noticed disagreeing.

moo-ds exports both as tokens and a `useBreakpoint` hook reading the same values its CSS uses, so a
consuming app never writes a pixel figure again.

## The filter shell

Three pieces, all generic enough that any filtered list wants them:

- **`Drawer` gains `placement="bottom"`.** It is already a portal-rendered overlay; this is a
  variant, not a new component.
- **`FilterBar`** — a flex row taking a primary slot (MooBank puts the period control there), a
  Filters button with a count badge, and a wrapping chip row beneath.
- **`FilterChip`** — a dismissible pill, the same shape moo-ds already uses for `CloseBadge`.

What goes *inside* the sheet stays with the consumer. A generic filter-field schema would mean
designing a DSL on the strength of one screen, and the result count needs the app's own query
regardless.

---

# Part two — MooBank

## The transactions page header

Neither existing header survives on a phone. `TransactionsAccountCard` is 224px and its
`@media (max-width: 768px)` rule drops `.stat-grid` to two columns, so three stats become 2 + 1 and
Net sits alone beside dead space. `TransactionsCompactWidgets` gives the balance the same tile,
border and type size as Net, and repeats the period label three times.

The mobile header is a new component, `TransactionsMobileHeader`, and it carries two facts:

```
┌──────────────────────────────────────────┐
│ BALANCE                             NET  │
│ $12,480.22                      +$1,276  │
└──────────────────────────────────────────┘
```

48px, one line, with the accent stripe `TransactionsAccountCard` already uses. Balance answers
*where am I*; net answers *was this period good or bad*, which is the only question income and
expenses were being used to answer. Net is signed and coloured, so it reads without parsing digits.

Everything else was already on screen twice, or does not belong:

| Was in the header | Now |
|---|---|
| Account name | The app bar title |
| Period label | The period control, one row below |
| Transaction count | The Filters button's result count |
| Type (Savings, Credit Card…) | Account settings — it never changes |
| Last transaction date | The first date separator in the list |
| Income and expenses | Derivable from net; available in reports |

The result is a page header smaller than the filter bar beneath it, which is the right order of
importance for a screen you came to read.

## The filter bar and sheet

Below `md`, `FilterPanel` and `MiniFilterPanel` are both replaced by moo-ds's `FilterBar` carrying:

- the period control as the primary slot — the filter changed most often, and the one that scopes
  the net figure above it;
- a **Filters** button badged with the count of non-period filters active;
- a chip per active filter, individually dismissible, with a Clear.

Tapping Filters opens a bottom `Drawer` holding description, tags, type as a segmented control, and
the untagged / exclude-offset / show-net switches at touch size. Its primary button carries the
result count — "Show 41 results" — so the effect is known before committing. That count is the
existing `useTransactions` query against the pending filter.

Filter state must remain visible without opening anything. A filter you cannot see is one you forget
you set, and then the figures look wrong and the app looks broken.

## Mobile is not compact mode

Compact mode looked like a shortcut and is not. Mobile takes a page header that exists in neither
view, a filter bar that exists in neither, and the two-column table from compact. It is its own
composition.

So `TransactionsAccountCard` and `TransactionsCompactWidgets` both become desktop-only, and the
Compact switch does not appear on mobile — not because mobile is always compact, but because it is
neither mode. `useLocalStorage("compact-mode")` is untouched, so the desktop preference survives.

`TransactionList` keeps its `compact` prop and its twin tables. Nothing there needs to change beyond
the breakpoint it reads.

## Fixes carried along

**A dead rule.** `src/App.css:190` intends to hide the actions on mobile:

```css
.container-fluid.second-header > .actions { display: none; }
```

moo-ds removed the container class, and says so in its own comment — *"Was
.container/.container-fluid"* — so this has matched nothing since that release. It goes.

**A collapsing input.** `src/css/filterPanel.css`:

```css
.mini-filter-panel #filter-desc { flex: 1 1 0; }
```

Basis `0` with no minimum, so once the row wraps `#filter-tags` (basis 175px) and `#filter-type`
(125px) hold their width and the search field is squeezed to nothing. It needs a real basis
(`1 1 12rem`). This still matters after the sheet lands, because `MiniFilterPanel` remains the
desktop compact filter.

**A hook with one call site.** `useIsDesktop` is used in exactly one place — `Transactions.tsx`,
to choose between `<TransactionList />` and `<TransactionList compact />`. But `TransactionList`
already carries `d-md-*` classes that do the same switch in CSS, so the variant is decided twice by
two mechanisms that could disagree. The hook call goes; the classes stay.

That leaves `useIsDesktop` and its tests unused, and they should move upstream rather than linger:
moo-ds's `useBreakpoint` replaces them, and `src/hooks/useMediaQuery.ts` is deleted with its test
file. `useMediaQuery` itself is worth keeping upstream — it is a general utility, and moo-ds is
where a general utility belongs once a second app needs it.

## Testing

moo-app already has `layout/__tests__/Header.test.tsx`; the mobile header joins it. The behaviour
worth asserting is the part that regressed silently before:

- a `PageAction` with `checked` renders as a menu item with a checked state, and `onClick` fires;
- `customActions` render in the bar, not the menu;
- the title renders the last breadcrumb only, never the trail;
- actions grouped `write` render below the divider.

In MooBank, the Vitest harness covers `TransactionsMobileHeader` (balance and net rendered with the
right sign and colour, including a negative net) and the filter bar (chip per active filter, count
excludes the period, dismissing a chip clears exactly that filter).

The header overlap itself is a layout bug that no unit test would have caught. It needs a look at
390pt in a browser before the PR, on both a light and a dark theme, since `--header-bg` and
`--breadcrumb-bg` diverge on `.light.red` and are identical on dark.

## Order of work

The moo-app change is breaking, so it lands first and MooBank follows in one bump.

1. **moo-ds** — breakpoint tokens and `useBreakpoint`; `Menu`; `Drawer placement="bottom"`;
   `FilterBar` and `FilterChip`; header CSS for the single mobile bar.
2. **moo-app** — `PageAction`, `customActions`, `LayoutProvider`, `ActionMenu`, the rewritten
   `Mobile/Header.tsx`; `UserMenu` moved onto the new `Menu`.
3. **MooBank** — bump; convert the `actions` arrays; `TransactionsMobileHeader`; the filter bar and
   sheet; the three fixes above.

## Open

- **The tag on a compact row.** The two-column table shows description and amount. If a phone is for
  reading, the tag is arguably the second most useful thing on the row — it turns a list of shop
  names into a picture of spending. It costs a second line, roughly three rows of the twelve. Worth
  trying against real data before deciding.
- **Other pages.** This spec designs the shell and one screen. Accounts, Budget and the report pages
  each need their own pass, and reports especially — a chart at 390pt is its own problem.
