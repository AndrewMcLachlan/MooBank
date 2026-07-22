---
paths:
  - "src/MooBank.Modules*/**"
---

# CQRS Implementation

## Overview

Command Query Responsibility Segregation (CQRS) separates read operations (Queries) from write operations (Commands).

## Commands

Commands represent write operations (Create, Update, Delete).

### Structure
- Located in `Commands/` folders within module projects
- Must implement `ICommand<TResult>`
- Handlers must implement `ICommandHandler<TCommand, TResult>`

### Implementation
```csharp
// Command definition
public record CreateTransaction(Guid InstrumentId, TransactionModel Model) : ICommand<Transaction>;

// Command handler
internal class CreateTransactionHandler(
    ITransactionRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransaction, Transaction>
{
    public async ValueTask<Transaction> Handle(CreateTransaction command, CancellationToken cancellationToken)
    {
        var entity = new TransactionEntity(command.Model);
        repository.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.ToModel();
    }
}
```

### Command Guidelines
- Can use custom model binding via `BindAsync` methods
- Should validate business rules
- Throw appropriate exceptions (`NotFoundException`, `BadHttpRequestException`)
- Use `IRepository<TEntity>` for data access (tracked entities)
- Commands deliberately return the full DTO of the created/updated resource (house style — a pragmatic divergence from purist CQRS)

## Queries

Queries represent read operations.

### Structure
- Located in `Queries/` folders within module projects
- Must implement `IQuery<TResult>`
- Handlers must implement `IQueryHandler<TQuery, TResult>`

### Implementation
```csharp
// Query definition
public record GetTransaction(Guid Id) : IQuery<Transaction>;

// Query handler
internal class GetTransactionHandler(
    IQueryable<TransactionEntity> transactions) : IQueryHandler<GetTransaction, Transaction>
{
    public async ValueTask<Transaction> Handle(GetTransaction query, CancellationToken cancellationToken)
    {
        var entity = await transactions
            .Specify(new TransactionDetailsSpecification())
            .SingleAsync(t => t.Id == query.Id, cancellationToken);
        return entity.ToModel();
    }
}
```

### Query Guidelines
- Should use specifications for complex filtering
- Can include pagination parameters (PageSize, PageNumber)
- Use `IQueryable<TEntity>` for data access (no tracking, read-only)

## Endpoints

Endpoints connect HTTP routes to Commands and Queries.

### Structure
- Defined in `Endpoints/` folders within module projects
- Use Minimal API syntax

### Implementation
```csharp
public static void MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
{
    var group = endpoints.MapGroup("/api/transactions")
        .WithTags("Transactions");

    // Map a query
    group.MapQuery<GetTransaction, Transaction>("/{id}")
        .RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));

    // Map a command
    group.MapCommand<CreateTransaction, Transaction>("/")
        .RequireAuthorization(Policies.GetInstrumentOwnerPolicy("instrumentId"));
}
```

### Endpoint Guidelines
- Use `.MapCommand()` and `.MapQuery()` extensions from `Postie.AspNetCore`
- Apply authorization policies using `.RequireAuthorization()`
- Note: the Transactions module intentionally applies `GetInstrumentViewerPolicy` to its whole
  endpoint group **including writes** (viewers may tag/annotate transactions). This is a settled
  decision — do not "tighten" it to the Owner policy.

## Module Structure

Each module in `MooBank.Modules.*` follows this structure:

```
Modules.{Feature}/
├── Commands/           # Write operations
│   ├── Create.cs
│   ├── Update.cs
│   └── Delete.cs
├── Queries/            # Read operations
│   ├── Get.cs
│   └── GetAll.cs
├── Models/             # DTOs specific to this module
├── Endpoints/          # API route definitions
│   └── {Feature}Endpoints.cs
└── Module.cs           # DI registration
```

## ASM Library

The CQRS implementation uses the ASM and Postie libraries:
- `Postie.Cqrs` / `Postie.Cqrs.AspNetCore` - CQRS mediator and endpoint dispatcher
- `Postie.AspNetCore` - Extensions for mapping endpoints to commands/queries
- `Asm.AspNetCore.Postie` - Paged query endpoint mapping
- Documentation: https://github.com/AndrewMcLachlan/ASM and https://github.com/AndrewMcLachlan/Postie
