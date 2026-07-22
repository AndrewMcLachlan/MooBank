# Postie Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dogfood Postie 1.0.0: add `Asm.AspNetCore.Postie` (paged-query mapping on Postie), migrate MooBank from Asm.Cqrs to Postie, then remove the Asm.Cqrs projects.

**Architecture:** Three sequenced PRs across two repos. Part A adds an additive Asm package exposing `MapPagedQuery` with Postie's full verb/binding surface, dispatching via the mediator-agnostic `IEndpointDispatcher`. Part B is the MooBank package/namespace/call-site migration with behaviour preservation (explicit `Parameters` binding where Asm's default applied). Part C deletes the two Asm.Cqrs projects as a major version.

**Tech Stack:** .NET 10, Postie 1.0.0 (nuget.org), Asm repo conventions (Reqnroll + Moq tests, `VersionPrefix` versioning), MooBank (xUnit, modules pattern).

**Spec:** `K:\Dev\Apps\MooBank\docs\superpowers\specs\2026-07-22-postie-migration-design.md`

## Global Constraints

- Asm repo (`K:\Dev\Libraries\Asm`): public OSS — no process docs committed; net10.0; `Nullable` enabled; XML docs on all public APIs; Reqnroll (`@Unit`-tagged scenarios) + Moq for tests; version = `VersionPrefix` in `Directory.Build.props` (patch is commit-count, do not hand-set).
- MooBank repo (`K:\Dev\Apps\MooBank`): docs committed with the branch; central package management (`Directory.Packages.props`); behaviour preservation is the migration rule — no endpoint semantics change beyond the documented deltas.
- Branches/PRs: Part A `feature/postie-extensions` (Asm), Part B `feature/postie` (MooBank), Part C `feature/remove-cqrs` (Asm). Each part ends at an open PR — the human merges; later parts wait for the earlier merge (Part B needs Part A's package on the GitHub feed).
- MooBank consumes `ASM.*` from GitHub Packages (see `NuGet.config` source mapping) and `Postie.*` from nuget.org.
- End every commit message with: `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`
- PR bodies end with: `🤖 Generated with [Claude Code](https://claude.com/claude-code)`

---

## Part A — Asm.AspNetCore.Postie (Asm repo)

### Task A1: Create the package with `MapPagedQuery`

**Files:**
- Create: `K:\Dev\Libraries\Asm\src\Asm.AspNetCore.Postie\Asm.AspNetCore.Postie.csproj`
- Create: `K:\Dev\Libraries\Asm\src\Asm.AspNetCore.Postie\AsmPostieEndpointRouteBuilderExtensions.cs`
- Modify: `K:\Dev\Libraries\Asm\Asm.slnx` (add the project; match existing entry format)
- Modify: `K:\Dev\Libraries\Asm\Directory.Build.props` (`<VersionPrefix>4.0</VersionPrefix>` → `4.1`)
- Modify: `K:\Dev\Libraries\Asm\Directory.Packages.props` if Postie needs a central version entry (check whether the repo uses CPM — if no `Directory.Packages.props` exists, put the version on the PackageReference)

**Interfaces:**
- Consumes: `Asm.PagedResult<T>` (`src/Asm/PagedResult.cs`: `Results` + `Total`), Postie.AspNetCore 1.0.0 public surface (`IEndpointDispatcher.DispatchAsync<T>`, `QueryMethod`, `RequestBinding`).
- Produces (Part B relies on exactly): `Asm.AspNetCore.AsmPostieEndpointRouteBuilderExtensions.MapPagedQuery<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, QueryMethod method = QueryMethod.Get, RequestBinding? binding = null) where TRequest : notnull` returning `RouteHandlerBuilder`.

- [ ] **Step 1: Branch**

```bash
cd K:/Dev/Libraries/Asm && git checkout -b feature/postie-extensions main
```

- [ ] **Step 2: Project file**

Create `src/Asm.AspNetCore.Postie/Asm.AspNetCore.Postie.csproj` (mirror a sibling like `src/Asm.Cqrs.AspNetCore/Asm.Cqrs.AspNetCore.csproj` for PropertyGroup conventions — read it first and copy its packaging metadata style):

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <Description>Postie endpoint mapping extensions using Asm conventions, starting with paged queries.</Description>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Postie.AspNetCore" Version="1.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Asm\Asm.csproj" />
  </ItemGroup>

</Project>
```

(Adjust to match sibling conventions exactly — e.g. if the repo centralises package versions, move `Version` accordingly; if siblings set `IsPackable`/README/icon items explicitly, copy that block.)

- [ ] **Step 3: The extension class**

Create `src/Asm.AspNetCore.Postie/AsmPostieEndpointRouteBuilderExtensions.cs`:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Postie.AspNetCore;

namespace Asm.AspNetCore;

/// <summary>
/// Maps paged CQRS queries to ASP.NET Core minimal API endpoints, dispatching through Postie's
/// mediator-agnostic <see cref="IEndpointDispatcher"/>.
/// </summary>
public static class AsmPostieEndpointRouteBuilderExtensions
{
    private const string QueryHttpMethod = "QUERY";

    /// <summary>
    /// Maps a query whose response is a <see cref="PagedResult{T}"/> to an endpoint that returns
    /// the unwrapped page with the total item count in an <c>X-Total-Count</c> response header.
    /// By default the endpoint is a GET bound from route, query and header values;
    /// <paramref name="method"/> selects POST or the HTTP QUERY method instead, both of which bind
    /// the query from the request body by default.
    /// </summary>
    /// <typeparam name="TRequest">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of each item in the page.</typeparam>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the route to.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="method">The HTTP method to map. Defaults to <see cref="QueryMethod.Get"/>.</param>
    /// <param name="binding">
    /// How the query is bound. Defaults to the idiomatic binding for <paramref name="method"/>:
    /// <see cref="RequestBinding.Parameters"/> for GET, <see cref="RequestBinding.Body"/> for POST
    /// and QUERY.
    /// </param>
    /// <returns>A <see cref="RouteHandlerBuilder"/> that can be used to further customise the endpoint.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="endpoints"/> or <paramref name="pattern"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="method"/> is not a defined <see cref="QueryMethod"/> value, or <paramref name="binding"/> is not a defined <see cref="RequestBinding"/> value.</exception>
    public static RouteHandlerBuilder MapPagedQuery<TRequest, TResponse>(this IEndpointRouteBuilder endpoints, string pattern, QueryMethod method = QueryMethod.Get, RequestBinding? binding = null) where TRequest : notnull
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(pattern);

        var handler = PagedHandler<TRequest, TResponse>(ResolveBinding(method, binding));

        RouteHandlerBuilder builder = method switch
        {
            QueryMethod.Get => endpoints.MapGet(pattern, handler),
            QueryMethod.Post => endpoints.MapPost(pattern, handler),
            QueryMethod.Query => endpoints.MapMethods(pattern, [QueryHttpMethod], handler),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, $"'{method}' is not a defined {nameof(QueryMethod)} value."),
        };

        return builder.Produces<IEnumerable<TResponse>>();
    }

    private static RequestBinding ResolveBinding(QueryMethod method, RequestBinding? binding)
    {
        if (binding is { } explicitBinding)
        {
            if (explicitBinding is not (RequestBinding.Default or RequestBinding.Body or RequestBinding.Parameters))
            {
                throw new ArgumentOutOfRangeException(nameof(binding), explicitBinding, $"'{explicitBinding}' is not a defined {nameof(RequestBinding)} value.");
            }

            return explicitBinding;
        }

        return method == QueryMethod.Get ? RequestBinding.Parameters : RequestBinding.Body;
    }

    // The binding attribute must sit on the delegate's request parameter for minimal API binding to
    // honour it, so a separate lambda is produced per binding.
    private static Delegate PagedHandler<TRequest, TResponse>(RequestBinding binding) where TRequest : notnull =>
        binding switch
        {
            RequestBinding.Body => async ([FromBody] TRequest request, HttpContext http, IEndpointDispatcher dispatcher, CancellationToken cancellationToken) =>
                await DispatchPaged<TRequest, TResponse>(request, http, dispatcher, cancellationToken),
            RequestBinding.Parameters => async ([AsParameters] TRequest request, HttpContext http, IEndpointDispatcher dispatcher, CancellationToken cancellationToken) =>
                await DispatchPaged<TRequest, TResponse>(request, http, dispatcher, cancellationToken),
            _ => async (TRequest request, HttpContext http, IEndpointDispatcher dispatcher, CancellationToken cancellationToken) =>
                await DispatchPaged<TRequest, TResponse>(request, http, dispatcher, cancellationToken),
        };

    private static async Task<IResult> DispatchPaged<TRequest, TResponse>(TRequest request, HttpContext http, IEndpointDispatcher dispatcher, CancellationToken cancellationToken) where TRequest : notnull
    {
        var result = await dispatcher.DispatchAsync<PagedResult<TResponse>>(request, cancellationToken);

        http.Response.Headers.Append("X-Total-Count", result.Total.ToString());
        return Results.Ok(result.Results);
    }
}
```

Known, accepted limitations (record in the PR body, not code comments): no Body-binding attribute-conflict guard and no null→404 path — Postie's `GuardBodyBinding`/`EndpointHandlers` are internal. This is dogfooding feedback: consider a future Postie public extension surface for third-party `Map*` authors.

- [ ] **Step 4: Build to verify it compiles**

```bash
dotnet build src/Asm.AspNetCore.Postie --configuration Release
```

Expected: success, 0 warnings. (Postie 1.0.0 restores from nuget.org.)

- [ ] **Step 5: Commit**

```bash
git add src/Asm.AspNetCore.Postie Asm.slnx Directory.Build.props
git commit -m "Add Asm.AspNetCore.Postie with MapPagedQuery on Postie's endpoint dispatcher

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

(Include `Directory.Packages.props` in the add list if it was edited.)

### Task A2: Tests (Reqnroll, TestServer round-trips)

**Files:**
- Create: `K:\Dev\Libraries\Asm\tests\Asm.AspNetCore.Postie.Tests\Asm.AspNetCore.Postie.Tests.csproj` (mirror `tests/Asm.Cqrs.AspNetCore.Tests/Asm.Cqrs.AspNetCore.Tests.csproj` — read it and copy its Reqnroll/Moq/xunit references and `reqnroll.json` if present)
- Create: `K:\Dev\Libraries\Asm\tests\Asm.AspNetCore.Postie.Tests\MapPagedQuery.feature`
- Create: `K:\Dev\Libraries\Asm\tests\Asm.AspNetCore.Postie.Tests\MapPagedQuerySteps.cs`
- Modify: `K:\Dev\Libraries\Asm\Asm.slnx`

**Interfaces:**
- Consumes: Task A1's `MapPagedQuery`; Postie's `AddPostie` registration (`Postie.Cqrs.AspNetCore` as a test-only dependency, or mock `IEndpointDispatcher` in DI — use the mock; the package under test must stay mediator-agnostic and the test proves it by never referencing a mediator).

- [ ] **Step 1: Write the feature**

`MapPagedQuery.feature`:

```gherkin
Feature: MapPagedQuery

@Unit
Scenario: GET paged query returns the page with a total count header
    Given a paged endpoint mapped with the default method
    And the dispatcher returns 2 items with 100 total
    When I GET the endpoint
    Then the response status should be 200
    And the response body should be the unwrapped items
    And the X-Total-Count header should be '100'

@Unit
Scenario: POST paged query binds the query from the body
    Given a paged endpoint mapped with method Post
    And the dispatcher returns 2 items with 40 total
    When I POST the criteria to the endpoint
    Then the response status should be 200
    And the X-Total-Count header should be '40'

@Unit
Scenario: QUERY-verb paged query binds the query from the body
    Given a paged endpoint mapped with method Query
    And the dispatcher returns 2 items with 7 total
    When I send a QUERY request with criteria to the endpoint
    Then the response status should be 200
    And the X-Total-Count header should be '7'

@Unit
Scenario: An undefined method value is rejected at map time
    Given a route builder
    When I map a paged endpoint with an undefined method value
    Then an ArgumentOutOfRangeException should be thrown for 'method'
```

- [ ] **Step 2: Write the steps**

`MapPagedQuerySteps.cs` — TestServer app with a `Mock<IEndpointDispatcher>` registered as singleton; the mock's `DispatchAsync<PagedResult<string>>(It.IsAny<object>(), It.IsAny<CancellationToken>())` returns `new PagedResult<string> { Results = ["item1", "item2"], Total = <per scenario> }`. Map with `app.MapPagedQuery<TestPagedQuery, string>("/paged"[, method])` where `private record TestPagedQuery(string? Term = null);` (no mediator interface — proves mediator-agnosticism). GET scenario calls `client.GetAsync("/paged")`; POST uses `client.PostAsJsonAsync("/paged", new TestPagedQuery("x"))`; QUERY uses `new HttpRequestMessage(new HttpMethod("QUERY"), "/paged") { Content = JsonContent.Create(new TestPagedQuery("x")) }`. Body assertion deserialises `List<string>` and compares to `["item1", "item2"]`. The undefined-method scenario calls `MapPagedQuery<TestPagedQuery, string>("/x", (QueryMethod)42)` inside `Assert.Throws<ArgumentOutOfRangeException>` and asserts `ParamName == "method"`. Follow `HandlersSteps.cs` structure for fixture/step-class layout (constructor-injected scenario context, `[Given]`/`[When]`/`[Then]` attributes with regex bindings matching the feature text verbatim).

- [ ] **Step 3: Run the new tests**

```bash
dotnet test tests/Asm.AspNetCore.Postie.Tests
```

Expected: all scenarios pass. (RED first if steps are written before Task A1 is referenced — feature/steps compile against the package, so failure mode here is compile-time; acceptable given A1 precedes.)

- [ ] **Step 4: Full repo build + test, commit**

```bash
dotnet build Asm.slnx --configuration Release && dotnet test Asm.slnx
git add tests/Asm.AspNetCore.Postie.Tests Asm.slnx
git commit -m "Cover MapPagedQuery: verbs, binding, header, guard

Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>"
```

Expected: 0 warnings, all existing suites still green.

### Task A3: PR

- [ ] Push and open the PR:

```bash
git push -u origin feature/postie-extensions
gh pr create --title "Add Asm.AspNetCore.Postie: MapPagedQuery on Postie" --body "<summary per spec: additive package, mediator-agnostic IEndpointDispatcher, full QueryMethod/RequestBinding surface, X-Total-Count contract preserved; version 4.0 -> 4.1; notes the two accepted limitations (no Body-binding guard, no null-to-404 — Postie internals not public) as Postie feedback.>

🤖 Generated with [Claude Code](https://claude.com/claude-code)"
```

**STOP — Part B waits until this PR is merged and CI has published 4.1.x to GitHub Packages.**

---

## Part B — MooBank migration (after A merges)

### Task B1: Package and namespace swap

**Files:**
- Modify: `K:\Dev\Apps\MooBank\Directory.Packages.props`
- Modify: all 15 `src\MooBank.Modules.*\MooBank.Modules.*.csproj`
- Modify: all 15 `src\MooBank.Modules.*\Properties\Global.cs`
- Commit also: `docs\superpowers\specs\2026-07-22-postie-migration-design.md` and this plan (already on disk, untracked)

- [ ] **Step 1: Branch**

```bash
cd K:/Dev/Apps/MooBank && git checkout -b feature/postie main
```

- [ ] **Step 2: Central package versions** — in `Directory.Packages.props`: remove `<PackageVersion Include="Asm.Cqrs.AspNetCore" Version="4.0.31" />`; add (Asm version = the actual 4.1.x CI published — check the GitHub Packages feed):

```xml
<PackageVersion Include="Asm.AspNetCore.Postie" Version="4.1.X" />
<PackageVersion Include="Postie.Cqrs.AspNetCore" Version="1.0.0" />
<PackageVersion Include="Postie.AspNetCore" Version="1.0.0" />
```

- [ ] **Step 3: Project references** — in each of the 15 module csprojs replace `<PackageReference Include="Asm.Cqrs.AspNetCore" />` with `<PackageReference Include="Postie.Cqrs.AspNetCore" />` and `<PackageReference Include="Postie.AspNetCore" />`; additionally add `<PackageReference Include="Asm.AspNetCore.Postie" />` ONLY in the modules with paged endpoints (find them: `grep -rl "MapPagedQuery" src --include="*.cs"` — expected: Transactions, Stocks and whichever others total 6 call sites across their endpoint files).

- [ ] **Step 4: Global usings** — in each `Properties\Global.cs`: `global using Asm.Cqrs.Commands;` → `global using Postie.Cqrs.Commands;` and `global using Asm.Cqrs.Queries;` → `global using Postie.Cqrs.Queries;`.

- [ ] **Step 5: Registration + dispatcher usings** — `grep -rn "using Asm.Cqrs" src tests --include="*.cs"` and update each remaining explicit using (Module.cs files if any, MCP tool classes, `tests\MooBank.Modules.Budgets.Tests\Support\TestMocks.cs`) to the Postie namespaces. `AddCommandHandlers`/`AddQueryHandlers` calls need no change beyond the using (same names in Postie; they live in `Microsoft.Extensions.DependencyInjection` namespace — verify none of the 15 `Module.cs` files need a new using at all).

- [ ] **Step 6: Verify zero remaining references**

```bash
grep -rn "Asm\.Cqrs" src tests --include="*.cs" --include="*.csproj" --include="*.props" | grep -v obj
```

Expected: no output.

### Task B2: Endpoint call-site migration

**Files:** the 44 endpoint files under `src\MooBank.Modules.*\Endpoints\`.

Transformation rules (apply mechanically, verify each with the greps below):
- [ ] `MapDelete<` → `MapDeleteCommand<` (14 sites; both arities).
- [ ] `CommandBinding.Body` → `RequestBinding.Body`; `CommandBinding.Parameters` → `RequestBinding.Parameters`; `CommandBinding.None` → `RequestBinding.Default` (add `using Postie.AspNetCore;` where the type is named).
- [ ] Every `MapCommand`/`MapPutCommand`/`MapPatchCommand`/`MapPostCreate`/`MapPutCreate`/`MapDeleteCommand` call **without** an explicit binding argument gets `binding: RequestBinding.Parameters` appended (behaviour preservation: Asm defaulted all verbs to Parameters, Postie defaults POST/PUT/PATCH to Body). Note: Postie's `MapPostCreate`/`MapPutCreate` parameter order matches Asm's (pattern, routeName, getRouteValues, binding) — spot-check one call against Postie's signature before sweeping.
- [ ] `MapPagedQuery` call sites (6): unchanged call shape (Task A1 preserved the signature); ensure the file's project has the `Asm.AspNetCore.Postie` reference (Task B1 Step 3).
- [ ] Verification greps — all must return zero:

```bash
grep -rn "CommandBinding" src --include="*.cs" | grep -v obj
grep -rn "MapDelete<" src --include="*.cs" | grep -v obj
```

And this must return exactly the same count as before migration (69+6+17+17+6+14+9 mapping calls — re-count with the same grep used pre-migration to prove no endpoint was dropped):

```bash
grep -rno "Map\(Paged\)\?Query<\|MapCommand<\|MapPutCommand<\|MapPatchCommand<\|MapPostCreate<\|MapPutCreate<\|MapDeleteCommand<" src --include="*.cs" | grep -v obj | wc -l
```

- [ ] Commit: `git add -- src docs && git commit -m "Migrate from Asm.Cqrs to Postie ..."` (explicit paths; include the spec + plan docs).

### Task B3: Build, tests, smoke

- [ ] `dotnet build MooBank.slnx --configuration Release` — 0 errors; triage warnings (new nullable warnings from Postie's annotations are fixable inline if trivial, otherwise report).
- [ ] `dotnet test MooBank.slnx` — all suites green (expect compile-only churn; the single behavioural test file is `TestMocks.cs`).
- [ ] Smoke checklist (run the app; requires Andrew's local environment — hand back to him if the app needs infrastructure that isn't running): one endpoint of each shape per the spec's verification gate — GET by id, paged GET (`X-Total-Count` + unwrapped body), POST-create (201 + Location), PATCH, PUT route-bound, DELETE void, 204/202 `MapCommand`, `IFormFile` upload, one MCP tool call, one `WithValidation` 400.
- [ ] Push, open PR titled "Migrate to Postie" with the deltas table from the spec in the body.

**STOP — Part C waits until the MooBank PR is validated and merged.**

---

## Part C — Asm.Cqrs removal (Asm repo, after B merges)

- [ ] Branch: `git checkout -b feature/remove-cqrs main` (after pulling the merged Part A).
- [ ] Delete `src/Asm.Cqrs`, `src/Asm.Cqrs.AspNetCore`, `tests/Asm.Cqrs.Tests`, `tests/Asm.Cqrs.AspNetCore.Tests`; remove all four from `Asm.slnx`.
- [ ] `grep -rn "Asm\.Cqrs" src tests --include="*.cs" --include="*.csproj" | grep -v obj` — expected: no output.
- [ ] `Directory.Build.props`: `<VersionPrefix>4.1</VersionPrefix>` → `5.0`.
- [ ] `dotnet build Asm.slnx --configuration Release && dotnet test Asm.slnx` — green, 0 warnings.
- [ ] Commit, push, PR titled "Remove Asm.Cqrs and Asm.Cqrs.AspNetCore (5.0)" — body: superseded by Postie; MooAuth/MooTrack stay on 4.x until migrated; migration recipe (Global.cs usings, `MapDelete`→`MapDeleteCommand`, `CommandBinding`→`RequestBinding`, explicit `Parameters` bindings, `MapPagedQuery` from `Asm.AspNetCore.Postie`); `CommandQueryController` check flagged for MooAuth/MooTrack.

## Completion

After each part: the PR is the gate — report and stop. No merging without Andrew.
