# Tag Visualizer Redesign — Design Spec

**Issue:** [#694 — Redo tag visualisation](https://github.com/AndrewMcLachlan/MooBank/issues/694)
**Date:** 2026-05-09
**Status:** Draft (awaiting review)

## 1. Problem

The current tag visualizer at `/tags/visualiser` (src/MooBank.Web.App/src/routes/tags/visualiser.tsx) renders the tag hierarchy as a static CSS `treeflex` tree. Observed problems:

- **Massive horizontal scroll.** Wide subtrees and a "small tags" row at the bottom run far past the viewport.
- **Multi-parent relationships are lost.** Tags can have multiple parents (e.g., "Sport" under both "Living Expense" and "Luxury Expense"), but the tree representation either drops links or shows the same subtree in multiple places without acknowledgement.
- **No navigation.** No expand/collapse, no search, no focus mode. With ~200 tags the page is a wall.
- **Wrong roots.** The backend `GetTagsHierarchy` query filters with `TaggedTo.Count != 0`, which returns *non-root* tags as starting points. The user-visible "trees" are actually orphan subtrees beginning at arbitrary depths.

## 2. Goals & Non-goals

### Goals (Phase 1)
- Show the **true graph** structure: every parent → child edge, including multi-parent relationships.
- **Vertical-only scrolling.** No horizontal overflow at any viewport ≥ 1024px wide.
- **Navigable.** Expand/collapse, search-to-filter, click-to-focus.
- Scale comfortably to ~500 tags with no performance hitches.

### Non-goals (Phase 2 / future)
- Inline editing (rename, recolour, link/unlink, delete).
- Insights / usage signals (tag frequency, tag spend totals).
- Full-canvas force-directed graph view.
- Drag-and-drop re-parenting.

## 3. UX Design

### 3.1 Layout

Two-pane layout on viewports ≥ 992px:

```
┌────────────────────────────────────────────────────────────┐
│  [breadcrumb]  Tags > Visualiser                           │
├────────────────────────┬───────────────────────────────────┤
│  🔍 search…            │   Neighborhood: Sport             │
│                        │                                   │
│  ▼ Living Expense      │       [Living]    [Luxury]        │
│    ▶ Medical           │           \      /                │
│    ▼ Sport ←focused    │           [Sport]                 │
│      • Tennis          │          /   |   \                │
│      • Skiing          │      [Tennis][Skiing][Hockey]     │
│      • Hockey          │                                   │
│    ▶ Insurance         │                                   │
│  ▼ Luxury Expense      │                                   │
│    ▼ Sport             │                                   │
│      • Tennis          │                                   │
│      • Skiing          │                                   │
│      • Hockey          │                                   │
└────────────────────────┴───────────────────────────────────┘
```

Below 992px: panes stack vertically (graph above outline). Outline is the primary control on narrow screens.

### 3.2 Outline behaviour

- **Roots:** tags with no parents (i.e., empty `TaggedTo`).
- **Multi-parent rendering:** "duplicate everywhere" — if `Sport` is a child of both `Living Expense` and `Luxury Expense`, it appears under both, with its own subtree fully expandable in each location. (User-confirmed; matches the data literally.)
- **Cycle protection:** never recurse into an ancestor on the current path. If a cycle is detected, render the node as a leaf with a small `↺` icon and tooltip "Cycle: …".
- **Default expansion:**
  - **First visit (no localStorage):** expand to first level (roots + their direct children).
  - **Returning visit:** restore expand/collapse state and last-focused tag from localStorage.
- **Focused tag:** highlighted with an outline border; the right-pane neighborhood graph reflects the focus.
- **Click handling:** clicking a row sets focus. Clicking the chevron toggles expand/collapse without changing focus.

### 3.3 Search

- Live filter-as-you-type, case-insensitive substring match on tag name.
- Non-matching subtrees are hidden, but **ancestors of any match are kept visible and auto-expanded** so matches stay reachable in their hierarchical context.
- Match text is highlighted (`<mark>`) within the chip.
- Clearing the search input restores the previous expand state.

### 3.4 Neighborhood graph (right pane)

Renders **a 1-hop neighborhood** of the focused tag using inline SVG:

- **Top row:** all direct parents of the focused tag (the tags in the focused tag's `TaggedTo` collection).
- **Center:** the focused tag (large chip, in its own colour).
- **Bottom row:** all direct children (tags in focused's `Tags`).
- Edges: straight lines from each parent to the center, and from center to each child.
- Layout is deterministic (alphabetical, evenly distributed) — no force simulation.
- Click a parent or child chip → it becomes the new focus (outline scrolls to nearest occurrence and highlights).
- Empty states:
  - No parents: top row collapses, center sits higher.
  - No children: bottom row collapses, center sits lower.
  - Tag has no relations at all: show a "no relations" message.

**Why custom SVG, not a graph library:**
- 1-hop neighborhoods are tiny (typically < 20 nodes). A force simulation is overkill and adds nondeterministic motion.
- Avoids a new dependency (`react-force-graph`, `cytoscape`).
- Renders predictably at any viewport. Easy to test.
- Trivial to swap for a library later if Phase 2 introduces a full-canvas view.

### 3.5 URL routing

- `/tags/visualiser` — outline-only on initial load (no focus until user clicks).
- `/tags/visualiser?focus=<tagId>` — page loads with that tag focused, outline scrolled to its first occurrence, neighborhood graph populated.
- Focus changes update the URL via `router.navigate` with `replace: true` (so the back button isn't polluted by every click).

### 3.6 Accessibility

- Outline is a `tree` ARIA role with `treeitem` rows and proper `aria-expanded`, `aria-level`, `aria-selected`.
- Keyboard: Up/Down to move focus, Left to collapse / move to parent, Right to expand / move to first child, Enter to focus the tag in the graph pane.
- Search input has `role="searchbox"` and announces match count via `aria-live="polite"`.
- Graph nodes are reachable by Tab; Enter activates re-focus.

## 4. Technical Design

### 4.1 Backend

The current `GetTagsHierarchy` query has flawed root selection and produces a flattened pseudo-tree. The new visualizer needs the raw graph so the frontend can present multiple views consistently.

**New query: `GetTagsGraph` returning `TagGraph`:**

```csharp
// MooBank.Modules.Tags/Queries/GetTagsGraph.cs
public record GetTagsGraph : IQuery<TagGraph>;

internal sealed class GetTagsGraphHandler(
    IQueryable<TagEntity> tags,
    User user) : IQueryHandler<GetTagsGraph, TagGraph>
{
    public async ValueTask<TagGraph> Handle(GetTagsGraph request, CancellationToken cancellationToken)
    {
        var rows = await tags
            .Where(t => t.FamilyId == user.FamilyId && !t.Deleted)
            .Include(t => t.Tags.Where(c => !c.Deleted))
            .Include(t => t.Settings)
            .AsSplitQuery()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Colour,
                t.Settings,
                ChildIds = t.Tags.Where(c => !c.Deleted).Select(c => c.Id).ToList(),
            })
            .ToListAsync(cancellationToken);

        var nodes = rows.Select(r => new TagNode(
            r.Id, r.Name, r.Colour, r.Settings.ToModel())).ToList();

        var edges = rows
            .SelectMany(r => r.ChildIds.Select(cid => new TagEdge(r.Id, cid)))
            .ToList();

        return new TagGraph(nodes, edges);
    }
}
```

```csharp
// MooBank.Modules.Tags/Models/TagGraph.cs
public record TagGraph(IReadOnlyList<TagNode> Nodes, IReadOnlyList<TagEdge> Edges);
public record TagNode(int Id, string Name, HexColour? Colour, TagSettings Settings);
public record TagEdge(int ParentId, int ChildId);
```

**Endpoint:** `GET /api/tags/graph` mapped via `MapQuery<GetTagsGraph, TagGraph>`. Same auth as `/api/tags/hierarchy`.

The existing `/api/tags/hierarchy` endpoint stays; other consumers (the breakdown report, etc.) keep using it. Once the new visualizer ships and is shown to be sufficient, we can audit removal of the hierarchy endpoint as a follow-up — out of scope for this issue.

### 4.2 Frontend

#### Files

```
src/MooBank.Web.App/src/routes/tags/
├── visualiser.tsx                     ← rewritten (currently the page component)
├── -components/
│   ├── TagsPage.tsx                   ← unchanged
│   ├── VisualiserOutline.tsx          ← new
│   ├── VisualiserOutlineRow.tsx       ← new
│   ├── VisualiserNeighborhood.tsx     ← new (SVG graph)
│   └── VisualiserSearch.tsx           ← new
├── -hooks/
│   ├── useTagsHierarchy.ts            ← unchanged
│   ├── useTagsGraph.ts                ← new
│   ├── useTagsGraphIndex.ts           ← new (memoised lookup maps + roots)
│   └── useVisualiserState.ts          ← new (expand state, focus, search; localStorage-backed)
└── -css/
    └── visualiser.css                 ← new (replaces the inline reliance on treeflex)
```

`treeflex.css` and `rainbow.css` stay until usage is confirmed elsewhere; the new visualizer does not depend on them. (A follow-up audit can remove them if no other consumer remains.)

#### Data flow

```
useTagsGraph() ── React Query ── /api/tags/graph
        │
        ▼
useTagsGraphIndex(graph)
   ├─ byId: Map<id, TagNode>
   ├─ childrenOf: Map<id, id[]>     // sorted by name
   ├─ parentsOf: Map<id, id[]>      // sorted by name
   └─ roots: id[]                   // nodes with no parents, sorted by name

useVisualiserState()
   ├─ expanded: Set<string>          // path keys, e.g. "1/4/12"
   ├─ focusId: number | null
   ├─ search: string
   └─ persist to localStorage on change (throttled)
```

A "path key" identifies a specific occurrence of a tag in the outline (since duplicates exist). It's the slash-joined chain of ancestor ids: `1/4/12`. This is what's stored in `expanded`. The same tag id under a different parent is a different path key.

#### Key components

**`Visualiser` (route component):**
- Reads `?focus=<id>` from the URL.
- Renders `<VisualiserSearch>`, `<VisualiserOutline>`, `<VisualiserNeighborhood>` in a CSS Grid layout.
- Keeps `focusId` synced with the URL (replaceState on change).

**`VisualiserOutline`:**
- Walks `roots` recursively, rendering `<VisualiserOutlineRow>` per node.
- Receives `expanded`, `focusId`, `search`, `onToggle`, `onFocus`.
- Cycle protection via an ancestor set passed down each branch.
- Search filtering via a precomputed "match map" (id → boolean) plus an "in-path-of-match" computation.

**`VisualiserOutlineRow`:**
- A single row: chevron, colour swatch, name (with search highlight), optional cycle indicator.
- ARIA `treeitem` semantics.

**`VisualiserNeighborhood`:**
- Pure SVG. Reads parents and children from `useTagsGraphIndex` for `focusId`.
- Layout function: positions parents along top edge, focus in middle, children along bottom edge. Even spacing, with vertical padding. SVG `<text>` for labels, `<rect>` for chips, `<line>` for edges.
- Click handlers on parent/child chips call `onFocus(id)`.

**`VisualiserSearch`:**
- Controlled `<input>` styled per the existing app conventions (no Bootstrap utility classes; uses semantic CSS).
- Debounce 150ms before mutating shared `search` state.

#### Styles

- New file `src/css/visualiser.css`, imported into `App.css` under `layer(moobank)`.
- Uses CSS Grid for the two-pane layout with a media query stacking it < 992px.
- No Bootstrap utility classes (per AGENTS.md).
- Tag chip colour comes from each tag's own `colour` field; falls back to a neutral palette token if missing.

### 4.3 Routing

The current `createFileRoute("/tags/visualiser")` stays, with a `validateSearch` added so `focus` is typed:

```ts
type VisualiserSearch = { focus?: number };

export const Route = createFileRoute("/tags/visualiser")({
    validateSearch: (s: Record<string, unknown>): VisualiserSearch =>
        s.focus !== undefined ? { focus: Number(s.focus) } : {},
    component: Visualiser,
});
```

Inside the component, `Route.useSearch()` returns `{ focus?: number }`. Focus changes call `navigate({ search: { focus: id }, replace: true })`.

### 4.4 Performance

- `useTagsGraphIndex` is memoised on the `TagGraph` reference. Building the indexes is `O(N + E)` where N = node count, E = edge count.
- Search match map is recomputed on `(graph, search)` change with `useMemo`.
- Outline rendering is virtualised only if profiling shows a problem; with ~500 nodes and CSS-only rows, react reconciliation should handle it.

## 5. Test Plan

### Backend
- Unit tests for `GetTagsGraphHandler`:
  - Filters by family.
  - Excludes deleted tags and edges to/from deleted tags.
  - Returns roots correctly (tags with no parents in the result set).
  - Multi-parent: a tag with two parents appears once in `Nodes`, twice in `Edges`.

### Frontend
- Component test: outline renders duplicated subtrees for multi-parent tags.
- Component test: cycle protection — A→B→A renders A's second appearance as a leaf with `↺`.
- Component test: search filters non-matching subtrees and auto-expands paths to matches.
- Component test: neighborhood graph renders parents above, children below; click re-focuses.
- Routing test: `?focus=42` loads with tag 42 focused; clicking another tag updates the URL.

## 6. Risks & Trade-offs

| Risk | Mitigation |
|------|------------|
| Duplicate-subtree rendering doubles render cost when a popular tag (e.g., "Sport") has many descendants and many parents. | Cap maximum render depth at 8 levels; deeper paths show a "…" leaf with a click-to-expand action. |
| Tag graph has cycles (data inconsistency). | Cycle detection in the recursion guard renders cycles safely as leaves with a marker. |
| `localStorage` state grows unbounded as users explore. | Persist only `expanded` path keys actively used in the current session; on save, drop keys that don't exist in the current graph. |
| User has very few tags and the outline feels overkill. | Empty state is fine: roots-only view for 5–10 tags is still readable. No special branch needed. |
| The new endpoint duplicates concerns with `GetTagsHierarchy`. | Keep both for now; flag a follow-up to consolidate once the hierarchy endpoint's other consumers are checked. |

## 7. Phase 2 (out of scope, design notes only)

Once Phase 1 ships, Phase 2 can layer in:

- **Inline rename** on the focused tag (right-pane action bar).
- **Recolour** via a colour picker.
- **Link / unlink parent** — drag a tag in the outline onto another tag, or use a "Add parent…" action.
- **Delete** with confirmation.

These all fit naturally on the right pane (a small action bar above the neighborhood graph) and reuse existing tag mutation endpoints. No structural rework needed.

## 8. Open questions

None blocking. Items deferred:

- Whether to remove `treeflex.css` / `rainbow.css` after this change — to be checked in a follow-up audit.
- Whether to retire `/api/tags/hierarchy` after migrating other consumers — out of scope.
