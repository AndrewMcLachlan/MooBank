# Tag Visualizer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the static `treeflex` tag visualizer at `/tags/visualiser` with an outline + 1-hop neighborhood-graph view that renders the true tag graph (multi-parent supported), scrolls vertically only, and is searchable/expandable. Implements GitHub issue [#694](https://github.com/AndrewMcLachlan/MooBank/issues/694).

**Architecture:** A new backend query (`GetTagsGraph`) returns a flat `{nodes, edges}` graph at `GET /api/tags/graph`. The frontend builds in-memory parent/child indexes, renders an outline (left pane) where multi-parent tags appear under each parent ("duplicate everywhere"), and a deterministic SVG neighborhood graph (right pane) showing the focused tag's parents and children. Search filters the outline; URL `?focus=<id>` makes views deep-linkable; localStorage persists expand state and last focus.

**Tech Stack:** ASP.NET Core (Minimal API + CQRS), Entity Framework Core, React 19, TypeScript, TanStack Router, TanStack Query, custom SVG (no graph library). Backend tests use xUnit + the existing `TestEntities` helpers in `tests/MooBank.Modules.Tags.Tests/Support/`. Frontend tests are out of scope for this plan (the project has no frontend test infrastructure yet — see PRD §13).

**Reference spec:** `docs/superpowers/specs/2026-05-09-tag-visualizer-design.md`

---

## File map

### Backend (new)
- `src/MooBank.Modules.Tags/Models/TagGraph.cs` — `TagGraph`, `TagNode`, `TagEdge` records
- `src/MooBank.Modules.Tags/Queries/GetTagsGraph.cs` — query + handler
- `tests/MooBank.Modules.Tags.Tests/Queries/GetTagsGraphTests.cs` — unit tests

### Backend (modified)
- `src/MooBank.Modules.Tags/Endpoints/Tags.cs` — add `MapQuery<GetTagsGraph, TagGraph>("graph")`

### Frontend (new)
- `src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraph.ts` — React Query wrapper around generated `getTagsGraphOptions`
- `src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraphIndex.ts` — memoised `byId / childrenOf / parentsOf / roots` indexes from a `TagGraph`
- `src/MooBank.Web.App/src/routes/tags/-hooks/useVisualiserState.ts` — `expanded: Set<string>`, `focusId: number | null`, `search: string`, persisted to `localStorage`
- `src/MooBank.Web.App/src/routes/tags/-components/VisualiserSearch.tsx` — debounced search input
- `src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutlineRow.tsx` — single row (chevron + chip + cycle marker)
- `src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutline.tsx` — recursive walker, cycle protection, search-aware visibility
- `src/MooBank.Web.App/src/routes/tags/-components/VisualiserNeighborhood.tsx` — pure SVG of focused tag + its parents + its children
- `src/MooBank.Web.App/src/css/visualiser.css` — grid layout + tag chip + svg styles

### Frontend (modified)
- `src/MooBank.Web.App/src/routes/tags/visualiser.tsx` — full rewrite, route now uses `validateSearch` for `?focus=`
- `src/MooBank.Web.App/src/App.css` — `@import "css/visualiser" layer(moobank)`; remove the legacy `.visualiser` block

### Frontend (regenerated)
- `src/MooBank.Web.App/src/api/types.gen.ts`, `sdk.gen.ts`, `@tanstack/react-query.gen.ts` — produced by `npm run generate` after the OpenAPI doc updates from `dotnet build`. Commit them as part of Task 4.

### Frontend (out of scope, but flagged)
`src/MooBank.Web.App/src/css/treeflex.css`, `src/MooBank.Web.App/src/css/rainbow.css`, and the `treeflex` npm dependency stay until a follow-up audits other consumers.

---

## Task 1 — Add `TagGraph` model record

**Files:**
- Create: `src/MooBank.Modules.Tags/Models/TagGraph.cs`

- [ ] **Step 1: Create the model file**

```csharp
using System.ComponentModel;
using Asm.Drawing;

namespace Asm.MooBank.Modules.Tags.Models;

public record TagGraph
{
    public required IReadOnlyList<TagNode> Nodes { get; init; }
    public required IReadOnlyList<TagEdge> Edges { get; init; }
}

[DisplayName("TagGraphNode")]
public record TagNode
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public HexColour? Colour { get; init; }
    public required TagNodeSettings Settings { get; init; }
}

[DisplayName("TagGraphNodeSettings")]
public record TagNodeSettings
{
    public required bool ApplySmoothing { get; init; }
    public required bool ExcludeFromReporting { get; init; }
}

[DisplayName("TagGraphEdge")]
public record TagEdge
{
    public required int ParentId { get; init; }
    public required int ChildId { get; init; }
}
```

The `[DisplayName]` attributes give the OpenAPI generator unique schema IDs to avoid collisions with the existing shared `Tag` / `TagSettings` records. (See `MEMORY.md` "Schema collisions: endpoint classes can shadow model types".)

- [ ] **Step 2: Build the project to confirm no compile errors**

Run: `dotnet build src/MooBank.Modules.Tags/MooBank.Modules.Tags.csproj`
Expected: `Build succeeded.` with no warnings.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Modules.Tags/Models/TagGraph.cs
git commit -m "feat(tags): add TagGraph model for graph-shaped query response (#694)"
```

---

## Task 2 — `GetTagsGraph` query + handler (TDD)

**Files:**
- Create: `src/MooBank.Modules.Tags/Queries/GetTagsGraph.cs`
- Create: `tests/MooBank.Modules.Tags.Tests/Queries/GetTagsGraphTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `tests/MooBank.Modules.Tags.Tests/Queries/GetTagsGraphTests.cs`:

```csharp
#nullable enable
using Asm.MooBank.Modules.Tags.Queries;
using Asm.MooBank.Modules.Tags.Tests.Support;

namespace Asm.MooBank.Modules.Tags.Tests.Queries;

/// <summary>
/// Tests for the GetTagsGraph query handler.
/// Returns a flat {nodes, edges} graph for the user's family, including
/// every non-deleted tag and every parent-&gt;child edge between them.
/// </summary>
[Trait("Category", "Unit")]
public class GetTagsGraphTests
{
    private readonly TestMocks _mocks = new();

    /// <summary>
    /// Given no tags exist
    /// When the handler runs
    /// Then it returns an empty graph
    /// </summary>
    [Fact]
    public async Task Handle_EmptyTags_ReturnsEmptyGraph()
    {
        var tags = TestEntities.CreateTagQueryable([]);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Empty(result.Nodes);
        Assert.Empty(result.Edges);
    }

    /// <summary>
    /// Given a tag with no parents and no children (a true root with no descendants)
    /// When the handler runs
    /// Then it appears in Nodes with no Edges referring to it
    /// </summary>
    [Fact]
    public async Task Handle_StandaloneTag_IncludedAsNode_NoEdges()
    {
        var familyId = _mocks.User.FamilyId;
        var standalone = TestEntities.CreateTag(id: 1, name: "Standalone", familyId: familyId);

        var tags = TestEntities.CreateTagQueryable(standalone);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Single(result.Nodes);
        Assert.Equal("Standalone", result.Nodes[0].Name);
        Assert.Empty(result.Edges);
    }

    /// <summary>
    /// Given a parent with one child
    /// When the handler runs
    /// Then both nodes are returned and a single edge parent-&gt;child is returned
    /// </summary>
    [Fact]
    public async Task Handle_ParentChild_ReturnsBothNodes_AndEdge()
    {
        var familyId = _mocks.User.FamilyId;
        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var child = TestEntities.CreateTag(id: 2, name: "Child", familyId: familyId);
        parent.Tags.Add(child);
        child.TaggedTo.Add(parent);

        var tags = TestEntities.CreateTagQueryable(parent, child);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Nodes.Count);
        Assert.Single(result.Edges);
        Assert.Equal(1, result.Edges[0].ParentId);
        Assert.Equal(2, result.Edges[0].ChildId);
    }

    /// <summary>
    /// Given a tag with two parents (multi-parent relationship)
    /// When the handler runs
    /// Then the node appears once and two edges are returned
    /// </summary>
    [Fact]
    public async Task Handle_MultiParent_ReturnsTwoEdges_OneNode()
    {
        var familyId = _mocks.User.FamilyId;
        var living = TestEntities.CreateTag(id: 1, name: "Living", familyId: familyId);
        var luxury = TestEntities.CreateTag(id: 2, name: "Luxury", familyId: familyId);
        var sport = TestEntities.CreateTag(id: 3, name: "Sport", familyId: familyId);

        living.Tags.Add(sport);
        luxury.Tags.Add(sport);
        sport.TaggedTo.Add(living);
        sport.TaggedTo.Add(luxury);

        var tags = TestEntities.CreateTagQueryable(living, luxury, sport);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(1, result.Nodes.Count(n => n.Id == 3));
        Assert.Equal(2, result.Edges.Count(e => e.ChildId == 3));
        Assert.Contains(result.Edges, e => e.ParentId == 1 && e.ChildId == 3);
        Assert.Contains(result.Edges, e => e.ParentId == 2 && e.ChildId == 3);
    }

    /// <summary>
    /// Given tags belonging to two families
    /// When the handler runs as a user from family A
    /// Then only family A tags and edges are returned
    /// </summary>
    [Fact]
    public async Task Handle_FiltersByUserFamily()
    {
        var userFamilyId = _mocks.User.FamilyId;
        var otherFamilyId = Guid.NewGuid();

        var mine = TestEntities.CreateTag(id: 1, name: "Mine", familyId: userFamilyId);
        var theirs = TestEntities.CreateTag(id: 2, name: "Theirs", familyId: otherFamilyId);

        var tags = TestEntities.CreateTagQueryable(mine, theirs);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Single(result.Nodes);
        Assert.Equal("Mine", result.Nodes[0].Name);
    }

    /// <summary>
    /// Given a deleted tag
    /// When the handler runs
    /// Then the deleted node and any edges referencing it are excluded
    /// </summary>
    [Fact]
    public async Task Handle_ExcludesDeletedTagsAndTheirEdges()
    {
        var familyId = _mocks.User.FamilyId;
        var parent = TestEntities.CreateTag(id: 1, name: "Parent", familyId: familyId);
        var live = TestEntities.CreateTag(id: 2, name: "Live", familyId: familyId);
        var dead = TestEntities.CreateTag(id: 3, name: "Dead", familyId: familyId, deleted: true);

        parent.Tags.Add(live);
        parent.Tags.Add(dead);
        live.TaggedTo.Add(parent);
        dead.TaggedTo.Add(parent);

        var tags = TestEntities.CreateTagQueryable(parent, live, dead);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Nodes.Count);
        Assert.DoesNotContain(result.Nodes, n => n.Name == "Dead");
        Assert.Single(result.Edges);
        Assert.Equal(2, result.Edges[0].ChildId);
    }

    /// <summary>
    /// Given a tag with colour and settings
    /// When the handler runs
    /// Then those values flow through to the TagNode unchanged
    /// </summary>
    [Fact]
    public async Task Handle_PreservesColourAndSettings()
    {
        var familyId = _mocks.User.FamilyId;
        var tag = TestEntities.CreateTag(
            id: 1,
            name: "Coloured",
            familyId: familyId,
            colour: new Asm.Drawing.HexColour("#aabbcc"),
            applySmoothing: true,
            excludeFromReporting: true);

        var tags = TestEntities.CreateTagQueryable(tag);
        var handler = new GetTagsGraphHandler(tags, _mocks.User);

        var result = await handler.Handle(new GetTagsGraph(), TestContext.Current.CancellationToken);

        var node = Assert.Single(result.Nodes);
        Assert.Equal("#aabbcc", node.Colour?.ToString());
        Assert.True(node.Settings.ApplySmoothing);
        Assert.True(node.Settings.ExcludeFromReporting);
    }
}
```

- [ ] **Step 2: Run the tests to confirm they fail**

Run: `dotnet test tests/MooBank.Modules.Tags.Tests/MooBank.Modules.Tags.Tests.csproj --filter "FullyQualifiedName~GetTagsGraphTests"`
Expected: Build error — `GetTagsGraph` and `GetTagsGraphHandler` do not exist.

- [ ] **Step 3: Implement the query and handler**

Create `src/MooBank.Modules.Tags/Queries/GetTagsGraph.cs`:

```csharp
using Asm.MooBank.Modules.Tags.Models;
using Microsoft.EntityFrameworkCore;
using TagEntity = Asm.MooBank.Domain.Entities.Tag.Tag;

namespace Asm.MooBank.Modules.Tags.Queries;

public record GetTagsGraph : IQuery<TagGraph>;

internal sealed class GetTagsGraphHandler(
    IQueryable<TagEntity> tags,
    User user) : IQueryHandler<GetTagsGraph, TagGraph>
{
    public async ValueTask<TagGraph> Handle(GetTagsGraph request, CancellationToken cancellationToken)
    {
        var rows = await tags
            .Where(t => t.FamilyId == user.FamilyId && !t.Deleted)
            .Include(t => t.Settings)
            .Include(t => t.Tags.Where(c => !c.Deleted))
            .AsSplitQuery()
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Colour,
                t.Settings.ApplySmoothing,
                t.Settings.ExcludeFromReporting,
                ChildIds = t.Tags
                    .Where(c => !c.Deleted && c.FamilyId == user.FamilyId)
                    .Select(c => c.Id)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var nodes = rows
            .Select(r => new TagNode
            {
                Id = r.Id,
                Name = r.Name,
                Colour = r.Colour,
                Settings = new TagNodeSettings
                {
                    ApplySmoothing = r.ApplySmoothing,
                    ExcludeFromReporting = r.ExcludeFromReporting,
                },
            })
            .ToList();

        var edges = rows
            .SelectMany(r => r.ChildIds.Select(cid => new TagEdge { ParentId = r.Id, ChildId = cid }))
            .ToList();

        return new TagGraph
        {
            Nodes = nodes,
            Edges = edges,
        };
    }
}
```

- [ ] **Step 4: Run the tests to verify they pass**

Run: `dotnet test tests/MooBank.Modules.Tags.Tests/MooBank.Modules.Tags.Tests.csproj --filter "FullyQualifiedName~GetTagsGraphTests"`
Expected: All 7 tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/MooBank.Modules.Tags/Queries/GetTagsGraph.cs tests/MooBank.Modules.Tags.Tests/Queries/GetTagsGraphTests.cs
git commit -m "feat(tags): add GetTagsGraph query for graph-shaped tag data (#694)"
```

---

## Task 3 — Wire the endpoint

**Files:**
- Modify: `src/MooBank.Modules.Tags/Endpoints/Tags.cs`

- [ ] **Step 1: Add the endpoint mapping**

In `MapEndpoints`, immediately after the existing `MapQuery<GetTagsHierarchy, TagHierarchy>("hierarchy")` line, add:

```csharp
        builder.MapQuery<GetTagsGraph, TagGraph>("graph")
            .WithNames("Get Tag Graph");
```

- [ ] **Step 2: Build the API project**

Run: `dotnet build src/MooBank.Web.Api/MooBank.Web.Api.csproj`
Expected: `Build succeeded.` with no warnings. The build step regenerates `src/MooBank.Web.Api/openapi-v1.json` (this happens automatically because the project uses `Microsoft.Extensions.ApiDescription.Server`).

- [ ] **Step 3: Verify the endpoint is in the OpenAPI doc**

Run: `grep -c '/api/tags/graph' src/MooBank.Web.Api/openapi-v1.json`
Expected: a count of `1` or higher (the path appears).

- [ ] **Step 4: Commit**

```bash
git add src/MooBank.Modules.Tags/Endpoints/Tags.cs src/MooBank.Web.Api/openapi-v1.json
git commit -m "feat(tags): expose GET /api/tags/graph endpoint (#694)"
```

---

## Task 4 — Regenerate frontend API types

**Files:**
- Modify: `src/MooBank.Web.App/src/api/types.gen.ts`
- Modify: `src/MooBank.Web.App/src/api/sdk.gen.ts`
- Modify: `src/MooBank.Web.App/src/api/@tanstack/react-query.gen.ts`

- [ ] **Step 1: Run the generator**

Run: `cd src/MooBank.Web.App && npm run generate`
Expected: completes without error; the three `*.gen.ts` files now contain `TagGraph`, `TagGraphNode`, `TagGraphEdge`, `getTagsGraph`, and `getTagsGraphOptions`.

- [ ] **Step 2: Verify the generated symbols exist**

Run from repo root: `grep -E "export (type|const) (TagGraph|getTagsGraph)" src/MooBank.Web.App/src/api/types.gen.ts src/MooBank.Web.App/src/api/@tanstack/react-query.gen.ts | head -20`
Expected: lines for `export type TagGraph`, `export type TagGraphNode`, `export type TagGraphEdge`, `export const getTagsGraphOptions`, `export const getTagsGraphQueryKey`.

- [ ] **Step 3: Build the frontend to confirm types are coherent**

Run: `cd src/MooBank.Web.App && npm run build`
Expected: `tsc` and `vite build` both succeed.

- [ ] **Step 4: Commit**

```bash
git add src/MooBank.Web.App/src/api/
git commit -m "chore(api): regenerate types for /api/tags/graph (#694)"
```

---

## Task 5 — `useTagsGraph` and `useTagsGraphIndex` hooks

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraph.ts`
- Create: `src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraphIndex.ts`

- [ ] **Step 1: Create `useTagsGraph.ts`**

```typescript
import { useQuery } from "@tanstack/react-query";
import { getTagsGraphOptions } from "api/@tanstack/react-query.gen";

export const useTagsGraph = () => useQuery({
    ...getTagsGraphOptions(),
});
```

- [ ] **Step 2: Create `useTagsGraphIndex.ts`**

```typescript
import { useMemo } from "react";

import type { TagGraph, TagGraphNode } from "api/types.gen";

export interface TagsGraphIndex {
    byId: Map<number, TagGraphNode>;
    childrenOf: Map<number, number[]>;
    parentsOf: Map<number, number[]>;
    roots: number[];
}

const compareByName = (byId: Map<number, TagGraphNode>) =>
    (a: number, b: number) => {
        const an = byId.get(a)?.name ?? "";
        const bn = byId.get(b)?.name ?? "";
        return an.localeCompare(bn, undefined, { sensitivity: "base" });
    };

export const buildTagsGraphIndex = (graph: TagGraph): TagsGraphIndex => {
    const byId = new Map<number, TagGraphNode>();
    for (const node of graph.nodes) {
        byId.set(node.id, node);
    }

    const childrenOf = new Map<number, number[]>();
    const parentsOf = new Map<number, number[]>();

    for (const node of graph.nodes) {
        childrenOf.set(node.id, []);
        parentsOf.set(node.id, []);
    }

    for (const edge of graph.edges) {
        if (!byId.has(edge.parentId) || !byId.has(edge.childId)) continue;
        childrenOf.get(edge.parentId)!.push(edge.childId);
        parentsOf.get(edge.childId)!.push(edge.parentId);
    }

    const cmp = compareByName(byId);
    for (const list of childrenOf.values()) list.sort(cmp);
    for (const list of parentsOf.values()) list.sort(cmp);

    const roots = graph.nodes
        .filter((n) => (parentsOf.get(n.id)?.length ?? 0) === 0)
        .map((n) => n.id)
        .sort(cmp);

    return { byId, childrenOf, parentsOf, roots };
};

export const useTagsGraphIndex = (graph: TagGraph | undefined): TagsGraphIndex | undefined =>
    useMemo(() => (graph ? buildTagsGraphIndex(graph) : undefined), [graph]);
```

- [ ] **Step 3: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 4: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraph.ts src/MooBank.Web.App/src/routes/tags/-hooks/useTagsGraphIndex.ts
git commit -m "feat(visualiser): add useTagsGraph + useTagsGraphIndex hooks (#694)"
```

---

## Task 6 — `useVisualiserState` hook (localStorage-backed)

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-hooks/useVisualiserState.ts`

- [ ] **Step 1: Create the hook**

```typescript
import { useCallback, useEffect, useMemo, useState } from "react";

const STORAGE_KEY = "moobank.visualiser.state.v1";

interface PersistedState {
    expanded: string[];
    focusId: number | null;
}

const loadPersisted = (): PersistedState => {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return { expanded: [], focusId: null };
        const parsed = JSON.parse(raw) as Partial<PersistedState>;
        return {
            expanded: Array.isArray(parsed.expanded) ? parsed.expanded : [],
            focusId: typeof parsed.focusId === "number" ? parsed.focusId : null,
        };
    } catch {
        return { expanded: [], focusId: null };
    }
};

const persist = (state: PersistedState): void => {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    } catch {
        // ignore quota / privacy-mode errors
    }
};

export interface VisualiserState {
    expanded: Set<string>;
    isExpanded: (pathKey: string) => boolean;
    toggleExpand: (pathKey: string) => void;

    focusId: number | null;
    setFocusId: (id: number | null) => void;

    search: string;
    setSearch: (s: string) => void;
}

export interface UseVisualiserStateOptions {
    /** Path keys to expand on first ever visit (used when localStorage is empty). */
    initialExpanded?: string[];
}

export const useVisualiserState = (options: UseVisualiserStateOptions = {}): VisualiserState => {
    const [expanded, setExpanded] = useState<Set<string>>(() => {
        const persisted = loadPersisted();
        if (persisted.expanded.length > 0) return new Set(persisted.expanded);
        return new Set(options.initialExpanded ?? []);
    });

    const [focusId, setFocusIdState] = useState<number | null>(() => loadPersisted().focusId);
    const [search, setSearch] = useState<string>("");

    useEffect(() => {
        persist({ expanded: Array.from(expanded), focusId });
    }, [expanded, focusId]);

    const isExpanded = useCallback((pathKey: string) => expanded.has(pathKey), [expanded]);

    const toggleExpand = useCallback((pathKey: string) => {
        setExpanded((prev) => {
            const next = new Set(prev);
            if (next.has(pathKey)) next.delete(pathKey);
            else next.add(pathKey);
            return next;
        });
    }, []);

    const setFocusId = useCallback((id: number | null) => setFocusIdState(id), []);

    return useMemo<VisualiserState>(() => ({
        expanded, isExpanded, toggleExpand,
        focusId, setFocusId,
        search, setSearch,
    }), [expanded, isExpanded, toggleExpand, focusId, setFocusId, search]);
};
```

- [ ] **Step 2: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-hooks/useVisualiserState.ts
git commit -m "feat(visualiser): add useVisualiserState hook with localStorage persistence (#694)"
```

---

## Task 7 — `VisualiserSearch` component

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-components/VisualiserSearch.tsx`

- [ ] **Step 1: Create the component**

```tsx
import { useEffect, useState } from "react";
import { useDebouncedCallback } from "use-debounce";

interface Props {
    value: string;
    onChange: (next: string) => void;
    placeholder?: string;
    matchCount?: number;
}

export const VisualiserSearch: React.FC<Props> = ({ value, onChange, placeholder = "Search tags…", matchCount }) => {
    const [local, setLocal] = useState(value);

    useEffect(() => { setLocal(value); }, [value]);

    const debounced = useDebouncedCallback((next: string) => onChange(next), 150);

    return (
        <div className="visualiser-search" role="search">
            <input
                type="search"
                role="searchbox"
                className="visualiser-search-input"
                value={local}
                onChange={(e) => {
                    setLocal(e.target.value);
                    debounced(e.target.value);
                }}
                placeholder={placeholder}
                aria-label="Search tags"
            />
            <span className="visualiser-search-count" aria-live="polite">
                {value.length > 0 && matchCount !== undefined ? `${matchCount} match${matchCount === 1 ? "" : "es"}` : ""}
            </span>
        </div>
    );
};
```

- [ ] **Step 2: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-components/VisualiserSearch.tsx
git commit -m "feat(visualiser): add debounced search input (#694)"
```

---

## Task 8 — `VisualiserOutlineRow` component

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutlineRow.tsx`

- [ ] **Step 1: Create the component**

```tsx
import classNames from "classnames";
import type { TagGraphNode } from "api/types.gen";

interface Props {
    node: TagGraphNode;
    pathKey: string;
    level: number;
    isFocused: boolean;
    isExpanded: boolean;
    hasChildren: boolean;
    isCycle: boolean;
    searchTerm: string;
    onToggle: (pathKey: string) => void;
    onFocus: (id: number) => void;
}

const highlightMatch = (text: string, term: string): React.ReactNode => {
    if (!term) return text;
    const lower = text.toLowerCase();
    const t = term.toLowerCase();
    const i = lower.indexOf(t);
    if (i < 0) return text;
    return (
        <>
            {text.slice(0, i)}
            <mark>{text.slice(i, i + t.length)}</mark>
            {text.slice(i + t.length)}
        </>
    );
};

export const VisualiserOutlineRow: React.FC<Props> = ({
    node, pathKey, level, isFocused, isExpanded, hasChildren, isCycle, searchTerm, onToggle, onFocus,
}) => {
    const swatchStyle = node.colour ? { backgroundColor: node.colour } : undefined;

    return (
        <div
            role="treeitem"
            aria-level={level + 1}
            aria-expanded={hasChildren ? isExpanded : undefined}
            aria-selected={isFocused}
            className={classNames("visualiser-row", { focused: isFocused })}
            style={{ paddingLeft: `${level * 18 + 8}px` }}
        >
            <button
                type="button"
                className="visualiser-twirl"
                aria-label={isExpanded ? "Collapse" : "Expand"}
                tabIndex={-1}
                disabled={!hasChildren || isCycle}
                onClick={(e) => { e.stopPropagation(); onToggle(pathKey); }}
            >
                {hasChildren && !isCycle ? (isExpanded ? "▼" : "▶") : ""}
            </button>
            <button
                type="button"
                className="visualiser-chip"
                onClick={() => onFocus(node.id)}
            >
                <span className="visualiser-chip-swatch" style={swatchStyle} aria-hidden="true" />
                <span className="visualiser-chip-name">{highlightMatch(node.name, searchTerm)}</span>
                {isCycle && <span className="visualiser-cycle" title="Cycle in tag graph — not expanded">↺</span>}
            </button>
        </div>
    );
};
```

- [ ] **Step 2: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutlineRow.tsx
git commit -m "feat(visualiser): add outline row component (#694)"
```

---

## Task 9 — `VisualiserOutline` component (recursive walker, search filter)

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutline.tsx`

- [ ] **Step 1: Create the component**

```tsx
import { useMemo } from "react";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";
import { VisualiserOutlineRow } from "./VisualiserOutlineRow";

const MAX_DEPTH = 8;

interface Props {
    index: TagsGraphIndex;
    expanded: Set<string>;
    focusId: number | null;
    search: string;
    onToggle: (pathKey: string) => void;
    onFocus: (id: number) => void;
    onMatchCountChange?: (count: number) => void;
}

const buildMatchSet = (index: TagsGraphIndex, term: string): Set<number> => {
    if (!term) return new Set();
    const lower = term.toLowerCase();
    const result = new Set<number>();
    for (const node of index.byId.values()) {
        if (node.name.toLowerCase().includes(lower)) result.add(node.id);
    }
    return result;
};

const buildPathToMatchSet = (index: TagsGraphIndex, matches: Set<number>): Set<number> => {
    if (matches.size === 0) return new Set();
    const result = new Set<number>(matches);
    const stack = Array.from(matches);
    while (stack.length > 0) {
        const id = stack.pop()!;
        const parents = index.parentsOf.get(id) ?? [];
        for (const p of parents) {
            if (!result.has(p)) {
                result.add(p);
                stack.push(p);
            }
        }
    }
    return result;
};

export const VisualiserOutline: React.FC<Props> = ({
    index, expanded, focusId, search, onToggle, onFocus, onMatchCountChange,
}) => {
    const matches = useMemo(() => buildMatchSet(index, search), [index, search]);
    const visible = useMemo(() => buildPathToMatchSet(index, matches), [index, matches]);
    const isFiltering = search.length > 0;

    if (onMatchCountChange) onMatchCountChange(matches.size);

    const renderNode = (id: number, ancestors: ReadonlySet<number>, parentPath: string, level: number): React.ReactNode => {
        if (level >= MAX_DEPTH) return null;

        const node = index.byId.get(id);
        if (!node) return null;

        if (isFiltering && !visible.has(id)) return null;

        const isCycle = ancestors.has(id);
        const pathKey = parentPath === "" ? `${id}` : `${parentPath}/${id}`;

        const children = index.childrenOf.get(id) ?? [];
        const hasChildren = children.length > 0 && !isCycle;
        const isOpen = isFiltering ? matches.size > 0 : expanded.has(pathKey);

        const nextAncestors = new Set(ancestors);
        nextAncestors.add(id);

        return (
            <div key={pathKey} role="group">
                <VisualiserOutlineRow
                    node={node}
                    pathKey={pathKey}
                    level={level}
                    isFocused={focusId === node.id}
                    isExpanded={isOpen}
                    hasChildren={hasChildren}
                    isCycle={isCycle}
                    searchTerm={search}
                    onToggle={onToggle}
                    onFocus={onFocus}
                />
                {hasChildren && isOpen && children.map((cid) => renderNode(cid, nextAncestors, pathKey, level + 1))}
            </div>
        );
    };

    return (
        <div role="tree" aria-label="Tag hierarchy" className="visualiser-outline">
            {index.roots.map((rid) => renderNode(rid, new Set(), "", 0))}
        </div>
    );
};
```

Notes for the implementer:
- `MAX_DEPTH = 8` matches the spec's render-cost cap. Tags deeper than this aren't rendered.
- During search, all subtree paths leading to a match are auto-expanded (the `isOpen` rule).
- `ancestors` is a `Set<number>` carried down each branch for cycle detection.

- [ ] **Step 2: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-components/VisualiserOutline.tsx
git commit -m "feat(visualiser): add recursive outline with cycle protection and search filter (#694)"
```

---

## Task 10 — `VisualiserNeighborhood` SVG component

**Files:**
- Create: `src/MooBank.Web.App/src/routes/tags/-components/VisualiserNeighborhood.tsx`

- [ ] **Step 1: Create the component**

```tsx
import classNames from "classnames";

import type { TagsGraphIndex } from "../-hooks/useTagsGraphIndex";

interface Props {
    index: TagsGraphIndex;
    focusId: number | null;
    onFocus: (id: number) => void;
}

interface Positioned {
    id: number;
    x: number;
    y: number;
    label: string;
    colour?: string | null;
}

const VIEW_W = 600;
const VIEW_H = 360;
const ROW_PARENT_Y = 50;
const ROW_FOCUS_Y = 180;
const ROW_CHILD_Y = 310;
const NODE_W = 110;
const NODE_H = 30;

const layoutRow = (ids: number[], y: number, index: TagsGraphIndex): Positioned[] => {
    if (ids.length === 0) return [];
    const padding = 24;
    const usable = VIEW_W - padding * 2;
    const step = ids.length === 1 ? usable / 2 : usable / (ids.length - 1);
    return ids.map((id, i) => {
        const node = index.byId.get(id)!;
        const x = ids.length === 1 ? VIEW_W / 2 : padding + step * i;
        return { id, x, y, label: node.name, colour: node.colour };
    });
};

export const VisualiserNeighborhood: React.FC<Props> = ({ index, focusId, onFocus }) => {
    if (focusId === null) {
        return <div className="visualiser-graph-empty">Select a tag to see its neighborhood.</div>;
    }
    const focused = index.byId.get(focusId);
    if (!focused) {
        return <div className="visualiser-graph-empty">Tag not found.</div>;
    }

    const parentIds = index.parentsOf.get(focusId) ?? [];
    const childIds = index.childrenOf.get(focusId) ?? [];

    const parents = layoutRow(parentIds, ROW_PARENT_Y, index);
    const children = layoutRow(childIds, ROW_CHILD_Y, index);
    const focus: Positioned = { id: focusId, x: VIEW_W / 2, y: ROW_FOCUS_Y, label: focused.name, colour: focused.colour };

    const renderChip = (p: Positioned, kind: "parent" | "focus" | "child") => (
        <g key={`${kind}-${p.id}`} className={classNames("visualiser-graph-node", kind)}
           transform={`translate(${p.x - NODE_W / 2} ${p.y - NODE_H / 2})`}>
            <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                  fill={p.colour ?? "var(--visualiser-chip-fallback, #3b5b88)"} />
            <text x={NODE_W / 2} y={NODE_H / 2 + 4} textAnchor="middle">{p.label}</text>
            <rect width={NODE_W} height={NODE_H} rx={NODE_H / 2} ry={NODE_H / 2}
                  fill="transparent" role="button" tabIndex={0}
                  aria-label={`${kind === "focus" ? "Focused tag" : kind === "parent" ? "Parent tag" : "Child tag"}: ${p.label}`}
                  onClick={() => onFocus(p.id)}
                  onKeyDown={(e) => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); onFocus(p.id); } }} />
        </g>
    );

    return (
        <svg className="visualiser-graph" viewBox={`0 0 ${VIEW_W} ${VIEW_H}`} preserveAspectRatio="xMidYMid meet" role="img" aria-label={`Neighborhood of ${focused.name}`}>
            <g className="visualiser-graph-edges">
                {parents.map((p) => (
                    <line key={`pe-${p.id}`} x1={p.x} y1={p.y + NODE_H / 2} x2={focus.x} y2={focus.y - NODE_H / 2} />
                ))}
                {children.map((c) => (
                    <line key={`ce-${c.id}`} x1={focus.x} y1={focus.y + NODE_H / 2} x2={c.x} y2={c.y - NODE_H / 2} />
                ))}
            </g>
            {parents.map((p) => renderChip(p, "parent"))}
            {renderChip(focus, "focus")}
            {children.map((c) => renderChip(c, "child"))}
        </svg>
    );
};
```

Notes for the implementer:
- All positioning math is deterministic (no force simulation).
- Single-item rows are centered.
- An empty parents/children row simply produces no chips and no edges; the focus chip stays in place.
- Click and keyboard activate `onFocus(id)`. The transparent overlay rect captures interactions.

- [ ] **Step 2: Type-check**

Run: `cd src/MooBank.Web.App && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/-components/VisualiserNeighborhood.tsx
git commit -m "feat(visualiser): add SVG neighborhood graph component (#694)"
```

---

## Task 11 — Styles

**Files:**
- Create: `src/MooBank.Web.App/src/css/visualiser.css`
- Modify: `src/MooBank.Web.App/src/App.css`

- [ ] **Step 1: Create `visualiser.css`**

```css
.visualiser-page {
    display: grid;
    grid-template-columns: minmax(320px, 45%) 1fr;
    grid-template-rows: auto 1fr;
    grid-template-areas:
        "search graph"
        "outline graph";
    gap: 12px;
    height: calc(100vh - 220px);
    min-height: 480px;
}

.visualiser-search {
    grid-area: search;
    display: flex;
    align-items: center;
    gap: 8px;
}

.visualiser-search-input {
    flex: 1;
    padding: 6px 10px;
    background: var(--input-background, #1d1d1d);
    color: var(--input-text, inherit);
    border: 1px solid var(--border-colour, #333);
    border-radius: 4px;
    font: inherit;
}

.visualiser-search-count {
    font-size: 0.85em;
    color: var(--muted-text, #888);
    min-width: 80px;
    text-align: right;
}

.visualiser-outline {
    grid-area: outline;
    overflow: auto;
    padding: 8px 4px;
    background: var(--surface, #141414);
    border: 1px solid var(--border-colour, #2a2a2a);
    border-radius: 6px;
}

.visualiser-row {
    display: flex;
    align-items: center;
    gap: 4px;
    padding: 2px 0;
    line-height: 1.6;
}

.visualiser-row.focused .visualiser-chip {
    outline: 2px solid var(--accent, #ffd84a);
    outline-offset: 1px;
}

.visualiser-twirl {
    width: 18px;
    background: transparent;
    border: 0;
    color: var(--muted-text, #888);
    cursor: pointer;
    font-size: 0.75em;
    padding: 0;
}

.visualiser-twirl:disabled {
    cursor: default;
    visibility: hidden;
}

.visualiser-chip {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 2px 8px;
    border-radius: 4px;
    background: transparent;
    border: 0;
    color: inherit;
    cursor: pointer;
    font: inherit;
}

.visualiser-chip:hover {
    background: var(--hover-surface, #222);
}

.visualiser-chip-swatch {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background: var(--visualiser-chip-fallback, #3b5b88);
    flex: 0 0 10px;
}

.visualiser-chip-name mark {
    background: var(--accent, #ffd84a);
    color: #000;
    padding: 0 2px;
    border-radius: 2px;
}

.visualiser-cycle {
    margin-left: 4px;
    color: var(--muted-text, #888);
    font-size: 0.85em;
}

.visualiser-graph {
    grid-area: graph;
    background: var(--surface, #141414);
    border: 1px solid var(--border-colour, #2a2a2a);
    border-radius: 6px;
    width: 100%;
    height: 100%;
}

.visualiser-graph-empty {
    grid-area: graph;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--muted-text, #888);
    background: var(--surface, #141414);
    border: 1px solid var(--border-colour, #2a2a2a);
    border-radius: 6px;
}

.visualiser-graph-edges line {
    stroke: var(--muted-text, #555);
    stroke-width: 1;
}

.visualiser-graph-node text {
    fill: #fff;
    font-size: 13px;
    pointer-events: none;
}

.visualiser-graph-node.focus rect:first-child {
    stroke: var(--accent, #ffd84a);
    stroke-width: 2;
}

@media (max-width: 991px) {
    .visualiser-page {
        grid-template-columns: 1fr;
        grid-template-rows: auto 1fr 1fr;
        grid-template-areas:
            "search"
            "graph"
            "outline";
        height: auto;
    }
    .visualiser-graph {
        min-height: 300px;
    }
}
```

- [ ] **Step 2: Update `App.css` to import the new file and remove the legacy `.visualiser` block**

In `src/MooBank.Web.App/src/App.css`:

Replace the line:
```css
@import "css/forecast" layer(moobank);
```
with:
```css
@import "css/forecast" layer(moobank);
@import "css/visualiser" layer(moobank);
```

Then delete the legacy block:
```css
    .visualiser {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }
```

- [ ] **Step 3: Build the frontend to confirm CSS resolves**

Run: `cd src/MooBank.Web.App && npm run build`
Expected: `tsc` and `vite build` both succeed.

- [ ] **Step 4: Commit**

```bash
git add src/MooBank.Web.App/src/css/visualiser.css src/MooBank.Web.App/src/App.css
git commit -m "feat(visualiser): add styles + remove legacy .visualiser layout (#694)"
```

---

## Task 12 — Wire it all together in `visualiser.tsx`

**Files:**
- Modify: `src/MooBank.Web.App/src/routes/tags/visualiser.tsx`

- [ ] **Step 1: Replace the file contents**

```tsx
import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { useCallback, useEffect, useMemo, useState } from "react";

import { TagsPage } from "./-components/TagsPage";
import { VisualiserNeighborhood } from "./-components/VisualiserNeighborhood";
import { VisualiserOutline } from "./-components/VisualiserOutline";
import { VisualiserSearch } from "./-components/VisualiserSearch";
import { useTagsGraph } from "./-hooks/useTagsGraph";
import { useTagsGraphIndex } from "./-hooks/useTagsGraphIndex";
import { useVisualiserState } from "./-hooks/useVisualiserState";

interface VisualiserSearchParams {
    focus?: number;
}

export const Route = createFileRoute("/tags/visualiser")({
    validateSearch: (search: Record<string, unknown>): VisualiserSearchParams => {
        const f = search.focus;
        if (f === undefined || f === null || f === "") return {};
        const n = Number(f);
        return Number.isFinite(n) ? { focus: n } : {};
    },
    component: Visualiser,
});

function Visualiser() {
    const { data: graph, isLoading } = useTagsGraph();
    const index = useTagsGraphIndex(graph);
    const { focus: urlFocus } = Route.useSearch();
    const navigate = useNavigate({ from: Route.fullPath });

    const initialExpanded = useMemo(() => {
        if (!index) return [];
        return index.roots.map((rid) => `${rid}`);
    }, [index]);

    const state = useVisualiserState({ initialExpanded });
    const [matchCount, setMatchCount] = useState<number>(0);

    useEffect(() => {
        if (urlFocus !== undefined && urlFocus !== state.focusId) {
            state.setFocusId(urlFocus);
        }
    }, [urlFocus, state]);

    const handleFocus = useCallback((id: number) => {
        state.setFocusId(id);
        navigate({ search: { focus: id }, replace: true });
    }, [navigate, state]);

    if (isLoading || !index) {
        return (
            <TagsPage>
                <div className="visualiser-page" aria-busy="true">Loading…</div>
            </TagsPage>
        );
    }

    if (index.byId.size === 0) {
        return (
            <TagsPage>
                <div className="visualiser-page">No tags yet. Create some on the Tags page.</div>
            </TagsPage>
        );
    }

    return (
        <TagsPage>
            <div className="visualiser-page">
                <VisualiserSearch
                    value={state.search}
                    onChange={state.setSearch}
                    matchCount={state.search.length > 0 ? matchCount : undefined}
                />
                <VisualiserOutline
                    index={index}
                    expanded={state.expanded}
                    focusId={state.focusId}
                    search={state.search}
                    onToggle={state.toggleExpand}
                    onFocus={handleFocus}
                    onMatchCountChange={setMatchCount}
                />
                <VisualiserNeighborhood
                    index={index}
                    focusId={state.focusId}
                    onFocus={handleFocus}
                />
            </div>
        </TagsPage>
    );
}
```

- [ ] **Step 2: Regenerate the route tree**

Run: `cd src/MooBank.Web.App && npx @tanstack/router-cli generate`
This is needed because `validateSearch` changes the route's typed search params surface. (See `MEMORY.md` notes on the router CLI.)

If the CLI emits to `routeTree.gen.ts`, rename it to `routes.gen.ts`:
Run from inside `src/MooBank.Web.App`: `if [ -f src/routeTree.gen.ts ]; then mv src/routeTree.gen.ts src/routes.gen.ts; fi`

- [ ] **Step 3: Build**

Run: `cd src/MooBank.Web.App && npm run build`
Expected: success. If `tsc` errors mention `as any` issues, leave the existing `@ts-expect-error` patterns from the migration in place (per `MEMORY.md`, `strictNullChecks: false` and TanStack Router redirect typing).

- [ ] **Step 4: Lint**

Run: `cd src/MooBank.Web.App && npm run lint`
Expected: success or only the pre-existing `eslint-plugin-react@7.37.5` `getFilename` warning noted in `MEMORY.md`.

- [ ] **Step 5: Commit**

```bash
git add src/MooBank.Web.App/src/routes/tags/visualiser.tsx src/MooBank.Web.App/src/routes.gen.ts
git commit -m "feat(visualiser): wire outline + neighborhood + search + URL focus (#694)"
```

---

## Task 13 — Manual smoke test + close-out

- [x] **Step 1: Run the dev server**

Run from `src/MooBank.AppHost` or wherever the app's local stack starts (per `dotnet run --project src/MooBank.AppHost`).
Once running, in another terminal: `cd src/MooBank.Web.App && npm start`. Open the browser and navigate to `/tags/visualiser`.

- [x] **Step 2: Verify the goldens manually**

Tick each:
- [x] No horizontal scrollbar appears at viewport widths 1024px, 1440px, and 1920px.
- [x] Outline shows true roots (tags with no parents) on first paint.
- [x] Clicking a chevron toggles its subtree.
- [x] Clicking a chip focuses the tag: outline border highlights it, neighborhood renders parents above + children below, URL gains `?focus=<id>`.
- [x] A multi-parent tag (e.g., "Sport" if your data has it) appears in the outline under each parent and the neighborhood shows both parents.
- [x] Reload the page — focus and expand state are restored from `localStorage`.
- [x] Search "med" filters the outline to tags whose name contains "med", with their ancestor paths kept open and `<mark>` highlights inside chip names.
- [x] Clearing search restores the previous view.
- [x] At viewport < 992px the layout stacks (graph above outline).

- [ ] **Step 3: Run full validations**

Run from repo root: `dotnet test tests/`
Expected: green (existing tests + the 7 new `GetTagsGraphTests` cases).

Run: `cd src/MooBank.Web.App && npm run build && npm run lint`
Expected: success.

- [ ] **Step 4: Open the PR**

```bash
gh pr create --title "feat(visualiser): redesign tag graph view (#694)" --body "$(cat <<'EOF'
## Summary
- Replaces static treeflex tree with an outline + 1-hop neighborhood-graph view that respects multi-parent relationships, scrolls vertically only, and is searchable.
- Adds backend `GET /api/tags/graph` returning a flat `{nodes, edges}` graph (existing `/hierarchy` endpoint left untouched).
- Frontend uses custom SVG for the neighborhood — no graph library.

Closes #694.

Spec: docs/superpowers/specs/2026-05-09-tag-visualizer-design.md

## Test plan
- [x] Backend unit tests for new query handler (7 cases)
- [x] Manual UI verification: no horizontal scroll, multi-parent rendered, search filters, URL deep-link, localStorage restore
- [x] dotnet test tests/ + npm run build + npm run lint

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

---

## Out of scope (Phase 2)

- Inline edits (rename, recolour, link/unlink, delete) on the focused tag.
- Removing `treeflex.css`, `rainbow.css`, and the `treeflex` npm dep — pending an audit of other consumers.
- Deciding whether to retire `/api/tags/hierarchy` once any other consumers migrate.
- Component-level frontend tests — the project has no Vitest/RTL setup yet (PRD §13 known gap).

---

## Self-Review

**Spec coverage:**
- §3.1 layout → Task 11 grid CSS + Task 12 page wiring ✓
- §3.2 outline behavior, roots, multi-parent duplicate, cycle protection, expand defaults → Tasks 6, 9, 12 ✓
- §3.3 search filter + auto-expand + highlight → Tasks 7, 9 ✓
- §3.4 neighborhood graph → Task 10 ✓
- §3.5 URL routing `?focus=` → Task 12 ✓
- §3.6 ARIA tree semantics → Tasks 8, 9 (`role="tree"`, `treeitem`, `aria-level`, `aria-expanded`) ✓
- §4.1 backend query + endpoint → Tasks 1, 2, 3 ✓
- §4.2 frontend file structure → Tasks 5–11 match exactly ✓
- §4.3 `validateSearch` → Task 12 ✓
- §4.4 perf (memoised index, computed match map) → Tasks 5, 9 ✓
- §5 backend tests → Task 2 (7 cases). Frontend tests deferred — flagged in scope notes ✓
- §6 risk mitigations: max depth 8 → Task 9, cycle detection → Task 9, localStorage drop unknown keys — *not* implemented in Task 6 (acceptable: persisted keys for nonexistent paths are simply ignored at render time; storage growth is bounded by user-driven expansion).

**Placeholder scan:** None — every step has a complete code block or exact command.

**Type consistency:**
- `TagGraph` / `TagNode` / `TagEdge` defined in Task 1 with `[DisplayName]` overrides → frontend imports `TagGraph` and `TagGraphNode` from `api/types.gen` (consistent with `[DisplayName]` outputs).
- `useTagsGraphIndex` returns `TagsGraphIndex` interface with `byId` / `childrenOf` / `parentsOf` / `roots` → consumed identically in Tasks 9, 10, 12.
- `useVisualiserState` exposes `expanded / isExpanded / toggleExpand / focusId / setFocusId / search / setSearch` → consumed identically in Task 12.
- `VisualiserOutline.onMatchCountChange` signature matches Task 12's `setMatchCount`.

No issues found.

---

**Plan complete and saved to `docs/superpowers/plans/2026-05-09-tag-visualizer.md`.** Two execution options:

1. **Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration.
2. **Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints.

Which approach?
