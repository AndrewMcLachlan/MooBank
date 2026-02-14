---
paths:
  - "src/Asm.MooBank.Infrastructure/**"
  - "src/Asm.MooBank.Domain/**"
---

# Entity Framework Core

## Data Access Patterns

### Unit of Work
- Always use the Unit of Work pattern for transactions
- Inject `IUnitOfWork` and call `SaveChangesAsync()` to persist changes

### Repositories
- Repositories should only be used for aggregate roots
- Use specifications for complex queries
- Interface defined in `Asm.MooBank.Domain/Entities/`
- Implementation in `Asm.MooBank.Infrastructure/Repositories/`

## IQueryable vs Repository

### 1. IQueryable<TEntity> - For Read Operations

- Injected directly for **read-only queries**
- Configured with `AsNoTracking()` for optimal performance
- Entities retrieved this way are **not tracked** by EF Core's change tracker
- **Use in Query handlers only** - perfect for fast, read-only operations
- Cannot be used to update entities (changes won't be persisted)

```csharp
// Query handler - uses IQueryable (no tracking, read-only)
internal class GetPlanHandler(IQueryable<ForecastPlan> plans, ...) : IQueryHandler<GetPlan, ForecastPlan>
{
    public async ValueTask<ForecastPlan> Handle(GetPlan query, ...)
    {
        var plan = await plans
            .Apply(new ForecastPlanDetailsSpecification())
            .SingleAsync(p => p.Id == query.Id, cancellationToken);
        return plan.ToModel();
    }
}
```

### 2. IRepository<TEntity> - For Write Operations

- Injected for **commands that modify data**
- Entities are tracked by EF Core's change tracker
- Changes to entities will be persisted when `IUnitOfWork.SaveChangesAsync()` is called
- **Use in Command handlers** that create, update, or delete entities
- Supports specifications for eager loading: `repository.Get(id, specification, cancellationToken)`

```csharp
// Command handler - uses Repository (tracked, can modify)
internal class UpdatePlanHandler(IForecastRepository forecastRepository, ...) : ICommandHandler<UpdatePlan, ForecastPlan>
{
    public async ValueTask<ForecastPlan> Handle(UpdatePlan request, ...)
    {
        var entity = await forecastRepository.Get(request.Id, new ForecastPlanDetailsSpecification(), cancellationToken);
        entity.Name = request.Plan.Name; // Changes will be tracked
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }
}
```

## Rule of Thumb

- **Queries** (read operations) → Use `IQueryable<TEntity>`
- **Commands** (write operations) → Use `IRepository<TEntity>`

## Specifications

- Used for complex query logic and filtering
- Implement `ISpecification<T>`
- Applied via `.Apply()` extension method
- Common patterns:
  - `IncludeXxxSpecification` - Eager loading related entities
  - `FilterSpecification` - Filtering criteria
  - `SortSpecification` - Ordering

## Best Practices

- Navigation properties should be explicitly loaded via specifications
- Avoid N+1 query problems by using `.Include()` or specifications
- Don't use EF migrations - schema changes go in the Database Project
