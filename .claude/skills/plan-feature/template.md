# Feature: <feature-name>

IMPORTANT: Validate documentation and codebase patterns before implementing. Pay special attention to naming of existing utils, types, and models. Import from the correct files.

## Feature Description

<Detailed description of the feature, its purpose, and value to users>

## User Story

As a <type of user>
I want to <action/goal>
So that <benefit/value>

## Feature Metadata

**Feature Type**: [New Capability / Enhancement / Refactor / Bug Fix]
**Estimated Complexity**: [Low / Medium / High]
**Primary Systems Affected**: [List of main components]
**Dependencies**: [External libraries or services required]

---

## Context References

### Mandatory Reading (READ BEFORE IMPLEMENTING)

<List files with line numbers and why they matter>

- `path/to/file.cs` (lines X-Y) - Why: Contains pattern to mirror
- `path/to/model.cs` (lines X-Y) - Why: Data model structure to follow

### New Files to Create

- `path/to/new_file.cs` - Purpose
- `path/to/new_file.ts` - Purpose

### External Documentation

- [Documentation](URL#section) - Why: Needed for X functionality

### Patterns to Follow

<Specific patterns extracted from codebase - include actual code examples from the project>

---

## Implementation Plan

### Phase 1: Foundation

<Foundational work: schemas, types, interfaces, dependencies>

### Phase 2: Core Implementation

<Main business logic, service layer, API endpoints, data models>

### Phase 3: Integration

<Connect to existing routers/handlers, register components, update config>

### Phase 4: Testing & Validation

<Unit tests, integration tests, edge cases>

---

## Step-by-Step Tasks

IMPORTANT: Execute every task in order, top to bottom. Each task is atomic and independently testable.

Task keywords:
- **CREATE**: New files or components
- **UPDATE**: Modify existing files
- **ADD**: Insert new functionality into existing code
- **REMOVE**: Delete deprecated code
- **MIRROR**: Copy pattern from elsewhere in codebase

### Task 1: {ACTION} {target_file}

- **IMPLEMENT**: {Specific implementation detail}
- **PATTERN**: {Reference to existing pattern - file:line}
- **IMPORTS**: {Required imports and dependencies}
- **GOTCHA**: {Known issues or constraints to avoid}
- **VALIDATE**: `{executable validation command}`

### Task 2: ...

<Continue with all tasks in dependency order>

---

## Testing Strategy

### Unit Tests

<Based on project testing patterns - Gherkin-style XML doc comments, xUnit, Moq>

### Integration Tests

<Authorization policy tests if applicable>

### Edge Cases

<Specific edge cases for this feature>

---

## Validation Commands

### Level 1: Build

```bash
dotnet build Asm.MooBank.slnx
cd src/Asm.MooBank.Web.App && npm run build
```

### Level 2: Tests

```bash
dotnet test tests/
cd src/Asm.MooBank.Web.App && npm test
```

### Level 3: Lint

```bash
cd src/Asm.MooBank.Web.App && npm run lint
```

---

## Acceptance Criteria

- [ ] Feature implements all specified functionality
- [ ] All validation commands pass with zero errors
- [ ] Tests cover new functionality
- [ ] Code follows project conventions (CQRS, DDD, module structure)
- [ ] No regressions in existing functionality
- [ ] No compiler warnings

---

## Notes

<Additional context, design decisions, trade-offs>
