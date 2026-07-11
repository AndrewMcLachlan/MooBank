---
paths:
  - "src/**/*.cs"
---

# C# Coding Conventions

## Naming Conventions

- **Entities**: PascalCase singular (e.g., `Transaction`, `LogicalAccount`)
- **Collections**: Use plural (e.g., `Transactions`, `Splits`)
- **Commands**: Verb-based names (e.g., `Create`, `Update`, `Delete`, `Close`)
- **Queries**: Descriptive names (e.g., `Get`, `GetAll`, `GetForMonth`)
- **DTOs/Models**: Match entity names but in the Models namespace

## Type Usage

- Use Framework types correctly:
  - `string` for declarations
  - `String` for static methods (e.g., `String.IsNullOrEmpty()`)
- Follow conventions defined in `.editorconfig`

## Error Handling

- Use `NotFoundException` when a requested resource doesn't exist
- Use `BadHttpRequestException` for invalid request data
- Use `UnauthorizedAccessException` or authorization policies for security violations
- Domain validation should throw domain-specific exceptions

## Code Quality

- **No Warnings**: Ensure code compiles without warnings
- Write clear, descriptive names for classes, methods, and variables
- Use domain-driven language that matches the business concepts

## Global Usings and Assembly Attributes

- Place global usings and assembly attributes in the relevant CSProj file, not code files

## File Organization

- Group related files by feature/entity, not by type
- Commands, Queries, and Models specific to a feature should be in that feature's folder
- Shared models belong in `MooBank.Models`

## Domain Model Guidelines

### Entities
- Located in `MooBank.Domain/Entities/`
- Should inherit from appropriate base classes (`Entity<TId>`, `AggregateRoot<TId>`)
- Contain business logic and invariants
- Use domain events for cross-aggregate communication
- Expose behavior through methods, not just property setters

### Specifications
- Used for complex query logic and filtering
- Located in `Specifications/` folders under entity folders
- Implement `ISpecification<T>`
- Should be composable and reusable
- Applied to an `IQueryable<T>` via the ASM `.Specify()` extension method
- Naming convention for new specifications: `XxxDetailsSpecification` for an include bundle
  (eager-loading a set of navigations), verb-named (e.g. `GetWithMembers`) for a filtered load

### Repositories
- Interface defined in `MooBank.Domain/Entities/`
- Implementation in `MooBank.Infrastructure/Repositories/`
- Should work with aggregate roots, not individual entities
- Use specifications for complex queries

## Domain Events

- Raised within aggregate roots during state changes
- Handlers live in the **Domain** project when they express cross-aggregate business logic
  (e.g. `MooBank.Domain/Entities/Transactions/EventHandlers/BalanceAdjustmentEventHandler.cs`
  creates an adjustment transaction), and in **Infrastructure** for technical concerns
  (e.g. `MooBank.Infrastructure/EventHandlers/InstrumentChangedEventHandler.cs` invalidates cache)
- Used for decoupled communication between aggregates
- Examples: `BalanceAdjustmentEvent` → creates an adjustment transaction; `InstrumentChangedEvent` → invalidates cached instrument data
- `TransactionAddedEvent` is raised but intentionally unconsumed — a deliberate extension point, not dead code. Do not remove it.

## Module Registration

- Modules must implement `IModule` from `Asm.AspNetCore.Modules`
- Register in `Program.cs` via `builder.RegisterModules()`
- Each module's `AddServices()` method registers its dependencies
- Each module's `MapEndpoints()` method defines its API endpoints
