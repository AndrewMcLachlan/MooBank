# Feature: Group Colours (GitHub issue #801)

IMPORTANT: Validate documentation and codebase patterns before implementing. Pay special attention to naming of existing utils, types, and models. Import from the correct files.

## Feature Description

Allow a user to assign a colour to a Group. The colour can be picked from a pre-defined palette of theme hues OR via a free-form colour picker (custom hex value). The account list page (the dashboard / "summary" account list) must visually surface the group colour — by colouring the group header and/or rendering a vertical stripe on the left edge of the group's table.

This mirrors the **existing, fully-implemented Tag colour feature** end-to-end (SQL `CHAR(7)` column → `HexColour` domain value object → `HexColourConverter` EF conversion → model property → `type="color"` input on the frontend). The implementation should follow that precedent closely to minimise risk.

## User Story

As a MooBank user
I want to assign a colour to each of my account groups
So that I can visually distinguish groups at a glance on the account list / dashboard.

## Feature Metadata

**Feature Type**: Enhancement
**Estimated Complexity**: Medium (full-stack: DB, domain, EF, module CQRS, API model, generated types, React form + display)
**Primary Systems Affected**:
- `MooBank.Database` (Group table schema)
- `MooBank.Domain` (Group entity)
- `MooBank.Infrastructure` (EF entity configuration for Group — new file)
- `MooBank.Modules.Groups` (Model, Create/Update commands, GetAll/Get queries via model extension)
- `MooBank.Modules.Instruments` (InstrumentsList `Group` model + `GetFormatted` query — to surface colour on the account list)
- `MooBank.Web.App` (regenerated API types, GroupForm, AccountListGroup, CSS)
- Tests: `MooBank.Modules.Groups.Tests`

**Dependencies**: No new external libraries. Reuses `Asm.Drawing.HexColour` (already referenced via `MooBank.Models` / `MooBank.Infrastructure`).

---

## Context References

### Mandatory Reading (READ BEFORE IMPLEMENTING)

The Tag colour feature is the canonical pattern to mirror. Read these first:

- `src/MooBank.Database/dbo/Tables/Tag.sql` (line 6) — Why: `[Colour] CHAR(7) NULL` column definition to copy into `Group.sql`.
- `src/MooBank.Domain/Entities/Tag/Tag.cs` (line 22) — Why: `public HexColour? Colour { get; set; }` property pattern on a domain entity.
- `src/MooBank.Infrastructure/EntityConfigurations/Tag.cs` (lines 12-16) — Why: shows the exact EF mapping `.HasConversion<HexColourConverter>().HasColumnType("char(7)").HasMaxLength(7)`. **A new `Group` EntityConfiguration must be created mirroring this.**
- `src/MooBank.Infrastructure/ValueConverters/HexColourConverter.cs` — Why: the converter already exists and is reused as-is.
- `src/MooBank.Models/Tag.cs` (line 18) — Why: `public HexColour? Colour { get; set; }` on a model record (serialised to the frontend).
- `src/MooBank.Web.App/src/routes/tags/-components/TagDetails.tsx` (lines 34-35) — Why: `<Input id="colour" type="color" className="form-control-color" value={(tag.colour as string) ?? ""} onChange={...} />` — the colour picker pattern. Note `HexColour` serialises as a string at runtime but is typed `unknown` in generated types, hence the `as string` cast.

Group feature files to modify:

- `src/MooBank.Domain/Entities/Group/Group.cs` (lines 13-26) — Group aggregate root. Add `Colour` property.
- `src/MooBank.Modules.Groups/Models/Group.cs` (whole file) — model record + `ToModel` mapping extension. Add `Colour`.
- `src/MooBank.Modules.Groups/Commands/Create.cs` (lines 8, 16-22) — `Create` record + handler. Add `Colour`.
- `src/MooBank.Modules.Groups/Commands/Update.cs` (lines 18-20) — handler copies model→entity. Add `Colour` copy.
- `src/MooBank.Modules.Groups/Commands/Validators.cs` — `CreateValidator` / `GroupValidator`. Optionally validate colour format.
- `src/MooBank.Infrastructure/MooBankContext.cs` (line 36) — `DbSet<Group> Groups`. Confirm EntityConfiguration is auto-applied (see Gotcha below).
- `src/MooBank.Modules.Instruments/Models/Instruments/InstrumentsList.cs` (lines 14-26) — the `InstrumentGroup` (DisplayName) model used by the account list. Add `Colour`.
- `src/MooBank.Modules.Instruments/Queries/Instruments/GetFormatted.cs` (lines 47-55) — builds the account-list `Group`. Add `Colour = ag.Colour`.
- `src/MooBank.Web.App/src/components/AccountList/AccountListGroup.tsx` (lines 16-41) — renders the group header + `SectionTable`. Add colour stripe/header styling.
- `src/MooBank.Web.App/src/routes/groups/-components/GroupForm.tsx` (lines 45-63) — Group create/edit form. Add colour input.
- `src/MooBank.Web.App/src/models/groups.ts` — `emptyGroup`. Add `colour` default (optional).
- `src/MooBank.Web.App/src/css/components/accountlist.css` (lines 109-198) — `table.accounts` styles; add stripe/header colour rules here.

Test references:

- `tests/MooBank.Modules.Groups.Tests/Support/TestEntities.cs` — `CreateGroup` / `CreateGroupModel` factory helpers. Add `colour` parameter.
- `tests/MooBank.Modules.Groups.Tests/Commands/UpdateTests.cs` — handler test pattern to extend with a colour assertion.
- `tests/MooBank.Modules.Groups.Tests/Commands/CreateTests.cs` — create handler tests.

### New Files to Create

- `src/MooBank.Infrastructure/EntityConfigurations/Group.cs` — EF `IEntityTypeConfiguration<Group>` to map the `Colour` property via `HexColourConverter` (mirrors `Tag.cs`).

### External Documentation

None required — `HexColour` lives in the `Asm.Drawing` namespace from the ASM NuGet package and is already in use.

### Patterns to Follow

**EF HexColour mapping (from `EntityConfigurations/Tag.cs:12-16`):**
```csharp
entity.Property(e => e.Colour)
    .HasConversion<HexColourConverter>()
    .HasColumnType("char(7)")
    .HasMaxLength(7);
```

**Domain entity property (from `Entities/Tag/Tag.cs:22`):**
```csharp
public HexColour? Colour { get; set; }
```

**Frontend colour input (from `tags/-components/TagDetails.tsx:34-35`):**
```tsx
<Input id="colour" type="color" className="form-control-color"
       value={(group.colour as string) ?? ""}
       onChange={(e) => /* set colour */} />
```

**Account-list group model build (from `Instruments/Queries/Instruments/GetFormatted.cs:47-55`):** the `new Group { ... }` projection where `ag` is the `Domain.Entities.Group.Group`.

---

## Implementation Plan

### Phase 1: Foundation (Database + Domain + EF)

1. Add `[Colour] CHAR(7) NULL` to the `Group` SQL table.
2. Add `HexColour? Colour` to the `Group` domain entity.
3. Create `GroupConfiguration` EF config mapping the colour column via `HexColourConverter`.

### Phase 2: Core Implementation (Module CQRS + Models)

4. Add `Colour` to the Groups module `Group` model + `ToModel` mapping.
5. Thread `Colour` through `Create` command + handler.
6. Thread `Colour` through `Update` handler.
7. (Optional) Add colour format validation.
8. Add `Colour` to the Instruments `InstrumentGroup` model and `GetFormatted` projection (so the account list receives it).

### Phase 3: Integration (Frontend)

9. Regenerate frontend API types (or hand-update if generation is unavailable).
10. Add a colour picker (+ predefined palette) to `GroupForm`.
11. Render the group colour on `AccountListGroup` (header tint + left stripe) with supporting CSS.
12. Update `emptyGroup` default.

### Phase 4: Testing & Validation

13. Update test factories + add colour-round-trip assertions to Create/Update handler tests.
14. Run full validation suite (build, test, lint).

---

## Step-by-Step Tasks

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

### Task 1: UPDATE `src/MooBank.Database/dbo/Tables/Group.sql`

- **IMPLEMENT**: Add a nullable colour column. Insert after the `[ShowPosition] BIT NOT NULL,` line:
  ```sql
  [Colour] CHAR(7) NULL,
  ```
- **PATTERN**: `src/MooBank.Database/dbo/Tables/Tag.sql:6` (`[Colour] CHAR(7) NULL`).
- **GOTCHA**: This is a Database Project (DACPAC). Do NOT create an EF migration. The column must be NULLable (no default) because existing groups have no colour. Keep the existing column order; just add the new line before the `CONSTRAINT` block.
- **VALIDATE**: `dotnet build src/MooBank.Database/MooBank.Database.sqlproj` (or build via `MooBank.slnx`).

### Task 2: UPDATE `src/MooBank.Domain/Entities/Group/Group.cs`

- **IMPLEMENT**: Add `public HexColour? Colour { get; set; }` to the `Group` class (e.g. after `ShowPosition`).
- **PATTERN**: `src/MooBank.Domain/Entities/Tag/Tag.cs:22`.
- **IMPORTS**: Add `using Asm.Drawing;` at the top of the file (the namespace for `HexColour`).
- **GOTCHA**: `HexColour` is a value type/struct in `Asm.Drawing` — use the nullable `HexColour?`.
- **VALIDATE**: `dotnet build src/MooBank.Domain/MooBank.Domain.csproj`.

### Task 3: CREATE `src/MooBank.Infrastructure/EntityConfigurations/Group.cs`

- **IMPLEMENT**: New `IEntityTypeConfiguration<Group>` that maps `Colour` via the converter:
  ```csharp
  using Asm.MooBank.Domain.Entities.Group;
  using Asm.MooBank.Infrastructure.ValueConverters;

  namespace Asm.MooBank.Infrastructure.EntityConfigurations;

  internal class GroupConfiguration : IEntityTypeConfiguration<Group>
  {
      public void Configure(EntityTypeBuilder<Group> entity)
      {
          entity.Property(e => e.Colour)
              .HasConversion<HexColourConverter>()
              .HasColumnType("char(7)")
              .HasMaxLength(7);
      }
  }
  ```
- **PATTERN**: `src/MooBank.Infrastructure/EntityConfigurations/Tag.cs:6-33` (use only the `Colour` mapping portion).
- **GOTCHA**: Confirm `MooBankContext` applies configurations from the assembly (look for `ApplyConfigurationsFromAssembly` in `MooBankContext.cs`). If it does, this file is auto-discovered. If configurations are registered individually, register `GroupConfiguration` the same way the others are. **Verify before assuming.** Also confirm there isn't already inline configuration for `Group` in `MooBankContext.cs` (entity currently uses data annotations `[AggregateRoot]`, `[PrimaryKey]` only — no `Colour` config exists yet).
- **VALIDATE**: `dotnet build src/MooBank.Infrastructure/MooBankInfrastructure.csproj` (or whole solution).

### Task 4: UPDATE `src/MooBank.Modules.Groups/Models/Group.cs`

- **IMPLEMENT**:
  - Add `public HexColour? Colour { get; init; }` to the `Group` record.
  - In `ToModel`, add `Colour = entity.Colour,`.
- **PATTERN**: `src/MooBank.Models/Tag.cs:18` for the property; existing `ToModel` in same file (lines 13-20).
- **IMPORTS**: Add `using Asm.Drawing;`.
- **GOTCHA**: The model is serialised to the OpenAPI spec → frontend types. `HexColour` serialises as a JSON string at runtime but is generated as `unknown` in TS (consistent with Tag — see `types.gen.ts:351 export type HexColour = unknown`).
- **VALIDATE**: `dotnet build src/MooBank.Modules.Groups/...csproj`.

### Task 5: UPDATE `src/MooBank.Modules.Groups/Commands/Create.cs`

- **IMPLEMENT**:
  - Change the record to: `public record Create(string Name, string Description, bool ShowTotal, HexColour? Colour) : ICommand<Models.Group>;`
  - In the handler, set `Colour = request.Colour` on the new `Group` entity.
- **PATTERN**: existing handler body (lines 16-22).
- **IMPORTS**: Add `using Asm.Drawing;`.
- **GOTCHA**: Adding a required positional record parameter changes the constructor; the frontend `useCreateGroup` currently sends `{ name, description, showTotal }` (see `useCreateGroup.ts:17`) — colour will arrive as `undefined`/null which is fine for a nullable param. Confirm no other callers construct `Create` directly.
- **VALIDATE**: `dotnet build` of the Groups module.

### Task 6: UPDATE `src/MooBank.Modules.Groups/Commands/Update.cs`

- **IMPLEMENT**: In `UpdateHandler.Handle`, after the existing property copies (lines 18-20) add:
  ```csharp
  entity.Colour = request.Group.Colour;
  ```
- **PATTERN**: lines 18-20 (entity.Name/Description/ShowPosition assignment).
- **VALIDATE**: `dotnet build` of the Groups module.

### Task 7: UPDATE `src/MooBank.Modules.Groups/Commands/Validators.cs` (optional but recommended)

- **IMPLEMENT**: Since `HexColour` is a typed value object that throws on invalid input during binding, explicit string-format validation is largely unnecessary. If `HexColour` binds nullable safely, you may skip. Otherwise add no rule (leave colour unvalidated) to avoid over-engineering.
- **PATTERN**: `GroupValidator` (lines 27-39).
- **GOTCHA**: Do NOT add a `MaximumLength`/regex rule against a `HexColour?` property — it's not a string. Skip validation here unless a clear need arises.
- **VALIDATE**: `dotnet build`.

### Task 8: UPDATE `src/MooBank.Modules.Instruments/Models/Instruments/InstrumentsList.cs` AND `Queries/Instruments/GetFormatted.cs`

- **IMPLEMENT**:
  - In `InstrumentsList.cs`, add `public HexColour? Colour { get; init; }` to the `[DisplayName("InstrumentGroup")] record Group` (after `Name`, around line 19). Add `using Asm.Drawing;`.
  - In `GetFormatted.cs`, in the `new Group { ... }` projection (lines 47-55), add `Colour = ag.Colour,`. `ag` is the `Domain.Entities.Group.Group`, which now has `Colour`.
- **PATTERN**: existing projection at `GetFormatted.cs:47-55`.
- **GOTCHA**: The "Other Accounts" pseudo-group (lines 57-67) has no backing entity (`Id = null`) — leave its `Colour` unset (null). Confirm `ag.Colour` is reachable: `GetGroup(userId)` returns the domain `Group` (see `Instrument.cs:52-53`), and `allGroups` is built from those, so `ag` carries the colour. Ensure the `LogicalAccount`/`StockHolding`/`Asset` group loading actually eager-loads the Group entity (it already does for Name/ShowPosition, so Colour comes along automatically).
- **VALIDATE**: `dotnet build src/MooBank.Modules.Instruments/...csproj`.

### Task 9: Regenerate / update frontend API types

- **IMPLEMENT**: After backend builds (which regenerates `openapi-v1.json` at build time), run the frontend type generation:
  ```bash
  cd src/MooBank.Web.App && npm run generate
  ```
  This updates `src/api/types.gen.ts` so `Group` and `InstrumentGroup` gain `colour?: null | HexColour`.
- **PATTERN**: existing Tag entries in `types.gen.ts:723,743,890` already show `colour?: null | HexColour`.
- **GOTCHA**: `npm run generate` reads the freshly built OpenAPI doc (`src/MooBank.Api/openapi-v1.json`). Build the backend FIRST. If generation tooling is unavailable in the environment, hand-edit `types.gen.ts`: add `colour?: null | HexColour;` to both the `Group` and `InstrumentGroup` interfaces (HexColour type alias already exists at line 351).
- **VALIDATE**: `cd src/MooBank.Web.App && npx tsc --noEmit` (or `npm run build`).

### Task 10: UPDATE `src/MooBank.Web.App/src/routes/groups/-components/GroupForm.tsx`

- **IMPLEMENT**: Add a colour `Form.Group` with BOTH a predefined-palette selector and a custom colour picker. Place it after the `showTotal` group (line 59), before the Save button. Minimal approach using react-hook-form's existing `form` + a `type="color"` input plus palette swatch buttons:
  ```tsx
  <Form.Group groupId="colour">
      <Form.Label>Colour</Form.Label>
      <div className="group-colour-picker">
          {groupColourPalette.map((c) => (
              <button
                  type="button"
                  key={c}
                  className={`colour-swatch${form.watch("colour") === c ? " selected" : ""}`}
                  style={{ backgroundColor: c }}
                  aria-label={c}
                  onClick={() => form.setValue("colour", c as any, { shouldDirty: true })}
              />
          ))}
          <input
              type="color"
              className="form-control form-control-color"
              value={(form.watch("colour") as string) ?? "#000000"}
              onChange={(e) => form.setValue("colour", e.target.value as any, { shouldDirty: true })}
          />
      </div>
  </Form.Group>
  ```
  Define the palette near the top of the file (or import from a shared util):
  ```tsx
  const groupColourPalette = ["#003f5c","#2f4b7c","#665191","#a05195","#d45087","#f95d6a","#ff7c43","#ffa600","#00876c","#d43d51"];
  ```
- **PATTERN**: colour input from `tags/-components/TagDetails.tsx:34-35`. Palette hues sourced from `src/MooBank.Web.App/src/utils/chartColours.ts:18-40` (`chartColours`) — you may import and reuse `chartColours` instead of redefining.
- **IMPORTS**: `chartColours` from `utils/chartColours` if reusing. `Form` is already imported from `@andrewmclachlan/moo-ds`.
- **GOTCHA**: `Group.colour` is typed `unknown` (HexColour) in generated types, so cast with `as any`/`as string` when reading/writing through react-hook-form (consistent with TagDetails). The form already spreads `data` on submit (`GroupForm.tsx:24-29`) so `colour` flows through automatically. Do NOT use Bootstrap utility classes — define `.group-colour-picker`, `.colour-swatch`, `.colour-swatch.selected` in CSS (Task 11).
- **VALIDATE**: `cd src/MooBank.Web.App && npm run lint`.

### Task 11: UPDATE `src/MooBank.Web.App/src/components/AccountList/AccountListGroup.tsx` + `src/css/components/accountlist.css`

- **IMPLEMENT (component)**: Use the group colour for a left stripe and a header tint. The `SectionTable` likely forwards `style`/`className`; if `style` is not supported, wrap or apply a CSS custom property. Recommended low-risk approach — set a CSS variable on the SectionTable and let CSS draw the stripe:
  ```tsx
  const colourStyle = group.colour
      ? ({ "--group-colour": group.colour as string } as React.CSSProperties)
      : undefined;
  // ...
  <SectionTable
      className={`accounts${group.colour ? " has-colour" : ""}`}
      style={colourStyle}
      hover header={headerContent} headerSize={2}
      hidden={group.instruments.length === 0}>
  ```
  If `SectionTable` does NOT accept `style`/`className` passthrough, instead apply the variable/stripe to an inline `style` on the `<header>` element inside `headerContent` (header background tint) — verify the `SectionTable` prop surface in `@andrewmclachlan/moo-ds` before choosing.
- **IMPLEMENT (CSS)** — add to `accountlist.css` inside/after the `table.accounts` block:
  ```css
  .section-table.accounts.has-colour {
      border-left: 4px solid var(--group-colour);
  }

  .section-table.accounts.has-colour > header {
      background-color: color-mix(in srgb, var(--group-colour) 12%, transparent);
      border-left: 4px solid var(--group-colour);
  }
  ```
  (Adjust selector to match the actual rendered DOM/class names produced by `SectionTable` — inspect at runtime or check moo-ds.)
- **PATTERN**: existing `table.accounts` rules (`accountlist.css:109-198`); `color-mix` is already used in this file (lines 4, 196).
- **GOTCHA**: `group.colour` is `unknown` (HexColour) — cast `as string`. The "Other Accounts" group has no colour (null) → the `has-colour` class is omitted and no stripe renders. Do NOT hard-code colours; drive everything from the `--group-colour` custom property. Confirm the exact class/element `SectionTable` renders (it may be `section.section-table` or similar) so the CSS selector matches.
- **VALIDATE**: `cd src/MooBank.Web.App && npm run build` then visually confirm.

### Task 12: UPDATE `src/MooBank.Web.App/src/models/groups.ts`

- **IMPLEMENT**: Leave `emptyGroup` without a colour (colour is optional/nullable) — no change strictly required. If you want a default, add `colour: undefined`. Prefer leaving it absent to avoid forcing a colour on new groups.
- **PATTERN**: existing `emptyGroup` (lines 4-8).
- **VALIDATE**: `npm run build`.

### Task 13: UPDATE test factories + add round-trip tests

- **IMPLEMENT**:
  - `tests/MooBank.Modules.Groups.Tests/Support/TestEntities.cs`: add a `HexColour? colour = null` parameter to `CreateGroup` and `CreateGroupModel`, and set `Colour = colour` in both. Add `using Asm.Drawing;`.
  - `tests/MooBank.Modules.Groups.Tests/Commands/UpdateTests.cs`: add a test (or extend an existing one) asserting `entity.Colour` is updated to the model's colour after `Handle`.
  - `tests/MooBank.Modules.Groups.Tests/Commands/CreateTests.cs`: add an assertion that the created entity/result carries the supplied colour.
- **PATTERN**: `UpdateTests.cs:47-73` (`Handle_ValidCommand_ModifiesEntityProperties`).
- **GOTCHA**: `HexColour` construction from a hex string is `new HexColour("#ff0000")`. Confirm the constructor signature against `Asm.Drawing` (the converter uses `new HexColour(v)` where `v` is the hex string — see `HexColourConverter.cs:10`).
- **VALIDATE**: `dotnet test tests/MooBank.Modules.Groups.Tests --filter "Category=Unit"`.

### Task 14: Full validation

- **VALIDATE**: Run all Level 1–3 validation commands below.

---

## Testing Strategy

### Unit Tests

Mirror existing Groups handler tests (xUnit, Moq, Gherkin XML doc comments per `.claude/rules/testing/backend.md`):
- `Create` handler persists `Colour` onto the new entity.
- `Update` handler copies `Colour` from model to entity.
- Round-trip: a `HexColour` set on the entity maps through `ToModel` to the model's `Colour`.

### Integration Tests

No new authorization surface — colour is set on an already-authorized Group via the existing `Update`/`Create` endpoints (already guarded by `Policies.GetGroupOwnerPolicy("id")`). No new integration tests required.

### Edge Cases

- Group with NULL colour (legacy data) → frontend renders no stripe, header untinted; account list "Other Accounts" group (Id null) has no colour.
- Custom colour outside the predefined palette → still accepted (free-form picker).
- Empty-string colour from the picker → ensure it maps to NULL (the converter already treats empty as null: `HexColourConverter.cs:10`). On the frontend, avoid sending `""`; send `undefined`/omit if cleared.
- Dark/light theme → use `color-mix` with `transparent` so the tint adapts.

---

## Validation Commands

### Level 1: Build

```bash
dotnet build MooBank.slnx
cd src/MooBank.Web.App && npm run build
```

### Level 2: Tests

```bash
dotnet test tests/
cd src/MooBank.Web.App && npm test
```

### Level 3: Lint

```bash
cd src/MooBank.Web.App && npm run lint
```

---

## Acceptance Criteria

- [ ] A colour can be assigned to a group via the Group form, choosing from a predefined palette OR a custom colour picker.
- [ ] The chosen colour persists to the database (`Group.Colour` CHAR(7)) and round-trips through the API.
- [ ] The account list page visually shows the group colour (left stripe and/or tinted header).
- [ ] Groups without a colour render normally (no stripe), including the "Other Accounts" group.
- [ ] All validation commands pass with zero errors and zero warnings.
- [ ] Unit tests cover Create/Update colour persistence.
- [ ] Code follows CQRS/DDD/module conventions; no Bootstrap utility classes in new CSS.

---

## Notes

- **Strongest precedent**: The Tag feature already implements hex colour storage with `HexColour` + `HexColourConverter` + `CHAR(7)`. Reuse it verbatim — do not invent a new colour representation.
- **Two surfaces consume Group**: (1) the Groups module model (`Modules.Groups/Models/Group.cs`) used by the Groups management pages, and (2) the Instruments `InstrumentGroup` model (`Modules.Instruments/.../InstrumentsList.cs`) used by the account list/dashboard. BOTH must expose `Colour`, but only the account-list one needs it for the visual stripe. Both flow from the same domain `Group.Colour`.
- **`HexColour` serialisation**: serialises to a JSON string but the OpenAPI generator emits `type HexColour = unknown` in TS. Follow the Tag precedent and cast `as string` in the frontend.
- **No EF migrations**: schema lives in the Database Project. Add the column to `Group.sql` only.
- **EF config discovery**: Verify how `MooBankContext` registers entity configurations before assuming `GroupConfiguration` is auto-applied (look for `ApplyConfigurationsFromAssembly`). This is the single most likely silent-failure point — if the config isn't applied, EF will try to map `HexColour` directly and fail or store it incorrectly.
- **`SectionTable` prop surface**: Confirm whether `SectionTable` (moo-ds) forwards `style`/`className` to the root element before finalising the stripe approach. If not, apply the colour to the inner `<header>` element instead.
- **Design decision (header vs stripe)**: The issue offers both options. This plan implements the left vertical stripe as the primary visual plus a subtle header tint — cheap, theme-safe via `color-mix`, and non-intrusive. Drop the header tint if it looks busy.
