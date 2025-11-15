# Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

# Key Concepts

- **Instrument** - The base type for all financial products (e.g., bank accounts, credit cards, loans, shares).
- **Transaction Instrument** - A subtype of Instrument that supports transactions (e.g., bank accounts, credit cards).
- **Logical Instrument** - Allows separation between the account and the financial product, allowing you to change institutions without changing accounts.
- **Virtual Instrument** - An Instrument that does not represent a real financial product but is used for tracking purposes (e.g., budget categories, savings goals).
- **Institution Account** - Represents a real account at a financial institution, linked to a Logical Instrument.
- **Tagging** - A way to categorize and organize transactions for better tracking and reporting.
- **Transaction Splits** - Allows a single transaction to be allocated across multiple categories or tags.
- **Recurring Transactions** - Scheduled transactions that are automatically created on Virtual Instruments.

# Architecture

## Technology Stack

The technology stack for MooBank includes:
- **Frontend**: React.js (TypeScript) for building a responsive and interactive user interface.
- **Backend**: .NET / ASP.NET Core. MooBank will generally use the latest general-availability version of .NET (currently .NET 9, targeting .NET 10).
- **Database**: Azure SQL Server with Entity Framework Core as the ORM.
- **API Documentation**: Microsoft.AspNetCore.OpenApi (not Swashbuckle) with Swagger UI for interactive documentation.

## Software Design Patterns

The software design patterns employed in MooBank include:

- **Minimal API** - Endpoints are defined using ASP.NET Core Minimal APIs.
- **Command Query Responsibility Segregation (CQRS)** - Commands (write operations) and Queries (read operations) are separated.
- **Domain-Driven Design (DDD)** - Business logic is encapsulated in domain entities with rich behavior.
- **Unit of Work and Repository Patterns** - For managing database transactions and data access.
- **Specification Pattern** - Used for composable query logic in repositories.
- **Domain Events** - For decoupled communication between aggregates.

## Code Layout

The codebase is organized into the following main projects:

### Core Projects
- **Asm.MooBank.Web.Api**: The ASP.NET Core Web API project. This is intentionally kept minimal, with most business logic in other projects.
- **Asm.MooBank.Domain**: Contains the core domain logic, including entities, value objects, aggregates, specifications, domain events, and repository interfaces. It specifically avoids dependencies on infrastructure concerns.
- **Asm.MooBank.Infrastructure**: Contains implementations for data access, including database context, repository implementations, entity configurations, and migrations.
- **Asm.MooBank.Models**: DTOs and models for API requests/responses.
- **Asm.MooBank**: Core business services and utilities.

### Module Projects
- **Asm.MooBank.Modules.***: A set of projects that encapsulate specific business capabilities. Each module contains:
  - Commands (write operations) with their handlers
  - Queries (read operations) with their handlers
  - Endpoint mappings
  - Module registration
  - Module-specific models (DTOs)

### Institution Integration Projects
- **Asm.MooBank.Institution.***: Projects for integrating with specific financial institutions (e.g., ING, AustralianSuper, Macquarie).
  - Each contains importers for parsing institution-specific data formats
  - Raw transaction storage for institutional data
  - Institution-specific domain entities

### Reference Data Integration Projects
- **Asm.MooBank.Abs**: Australian Bureau of Statistics integration for inflation data.
- **Asm.MooBank.ExchangeRateApi**: Integration for currency exchange rates.
- **Asm.MooBank.Eodhd**: Integration for stock price data.

### Supporting Projects
- **Asm.MooBank.Security**: Authentication and authorization logic.
- **Asm.MooBank.Web.Jobs**: Background job implementations (Azure WebJobs).
- **Asm.MooBank.AppHost**: Aspire AppHost for local development orchestration.

### Frontend
- **Asm.MooBank.Web.App**: React/TypeScript single-page application located in `src/Asm.MooBank.Web.App`.

### Database
- **Asm.MooBank.Database**: SQL Server Database Project (SDK-style) containing schemas, tables, functions, and stored procedures.

## Libraries and Frameworks

### Backend

- **Entity Framework Core**: An Object-Relational Mapper (ORM) for database access.
- **ASM Library**: A set of custom NuGet packages developed to hide boilerplate and simplify common tasks:
    - `Asm.AspNetCore.Api` - For API-specific functionality including OpenAPI configuration.
    - `Asm.AspNetCore` - For application bootstrapping, including OpenTelemetry and health checks.
    - `Asm.AspNetCore.Modules` - Module registration and endpoint mapping infrastructure.
    - `Asm.Cqrs.AspNetCore` - Extensions that allow endpoints to be mapped to CQRS commands and queries.
    - `Asm.Domain` / `Asm.Domain.Infrastructure` - Base classes and interfaces for implementing DDD patterns.
    - `Asm.OAuth` - OAuth/OIDC configuration helpers.
  
  The code for the ASM Library is available at: https://github.com/AndrewMcLachlan/ASM

- **ReqnRoll**: A behavior-driven development (BDD) framework for writing human-readable integration tests.
- **Database Project**: The database design is maintained as a first-class component using the newer SDK-style SQL project format.

### Frontend

- **React.js**: A JavaScript library for building user interfaces (using TypeScript).
- **React Router**: A library for routing in React applications.
- **MSAL (Microsoft Authentication Library)**: For handling authentication and authorization with Azure AD.
- **React Query (TanStack Query)**: For server state management and API calls.
- **MooApp**: A set of custom packages of React components and hooks to simplify application development. The code for MooApp is available at: https://github.com/AndrewMcLachlan/MooApp

## CQRS Implementation Guidelines

### Commands
- Commands represent write operations (Create, Update, Delete).
- Located in `Commands/` folders within module projects.
- Must implement `ICommand<TResult>`.
- Handlers must implement `ICommandHandler<TCommand, TResult>`.
- Can use custom model binding via `BindAsync` methods.
- Should validate business rules and throw appropriate exceptions (`NotFoundException`, `BadHttpRequestException`).

### Queries
- Queries represent read operations.
- Located in `Queries/` folders within module projects.
- Must implement `IQuery<TResult>`.
- Handlers must implement `IQueryHandler<TQuery, TResult>`.
- Should use specifications for complex filtering.
- Can include pagination parameters (PageSize, PageNumber).

### Endpoints
- Defined in `Endpoints/` folders within module projects.
- Use Minimal API syntax with `.MapCommand()` and `.MapQuery()` extensions from `Asm.Cqrs.AspNetCore`.
- Apply `.WithOpenApi()` for API documentation.
- Apply authorization policies using `.RequireAuthorization()`.

## Domain Model Guidelines

### Entities
- Located in `Asm.MooBank.Domain/Entities/`.
- Should inherit from appropriate base classes (`Entity<TId>`, `AggregateRoot<TId>`).
- Contain business logic and invariants.
- Use domain events for cross-aggregate communication.
- Expose behavior through methods, not just property setters.

### Specifications
- Used for complex query logic and filtering.
- Located in `Specifications/` folders under entity folders.
- Implement `ISpecification<T>`.
- Should be composable and reusable.
- Common examples: `FilterSpecification`, `IncludeXxxSpecification`, `SortSpecification`.

### Repositories
- Interface defined in `Asm.MooBank.Domain/Entities/`.
- Implementation in `Asm.MooBank.Infrastructure/Repositories/`.
- Should work with aggregate roots, not individual entities.
- Use specifications for complex queries.

## Security

- **Authentication**: OAuth 2.0 and OpenID Connect via Azure AD for secure user authentication.
- **Authorization**: Policy-based authorization to control access to resources based on user roles and permissions.
  - Policies are defined in `Asm.MooBank.Security`.
  - Applied at the endpoint level.
- **Multi-tenancy**: Users are grouped into Families and Groups for data isolation.

## Hosting

The application is hosted on Microsoft Azure using the following services:
- **Azure App Service**: For hosting the web application and API.
    - **Deployment Slots**: Used for staging and production environments to facilitate smooth deployments.
- **Azure SQL Database**: For the application database.
- **Azure WebJobs**: For background job processing.
- **Key Vault**: For securely storing application secrets and configuration settings.
- **Application Insights**: For monitoring and telemetry.

## Build and Deployment

- The build and deployment workflow is defined in `.github/workflows/build.yml`.
- Uses GitHub Actions for CI/CD.
- Automated deployment to Azure App Service.
- Database migrations are applied automatically during deployment.

## Integrations

### Financial Institution Integrations
These are used to connect to external financial institutions to retrieve account and transaction data:
- Generally involve upload and parsing of CSV files via the `/api/instruments/{id}/import` endpoint.
- Each institution has its own project (e.g., `Asm.MooBank.Institution.Ing`).
- Implement the `IImporter` interface.
- Store raw transaction data for reprocessing capability.
- Located in `Asm.MooBank.Institution.*` projects.

### Reference Data Integrations
These are used to retrieve reference data:
- **Currency exchange rates**: Via `Asm.MooBank.ExchangeRateApi`.
- **Inflation data**: Via `Asm.MooBank.Abs` (Australian Bureau of Statistics).
- **Stock prices**: Via `Asm.MooBank.Eodhd`.
- Typically use REST APIs.
- Run as scheduled background jobs.

## Background Jobs

Background jobs are implemented as Azure WebJobs in `Asm.MooBank.Web.Jobs`:

- **Recurring Transactions**: Regularly creates transactions against Virtual Instruments on a set schedule.
- **Reference Data Updates**: Regularly updates reference data such as currency exchange rates, inflation data, and stock prices.
- Jobs are triggered on a schedule using CRON expressions.

## Testing

- **Unit/Integration Tests**: Located in `tests/` directory.
- **BDD Tests**: Using ReqnRoll with `.feature` files for human-readable specifications.
- Test projects mirror the structure of the main projects.
- Use mocks and fakes for external dependencies.

## Common Patterns and Conventions

### Naming Conventions
- **Entities**: PascalCase singular (e.g., `Transaction`, `LogicalAccount`).
- **Collections**: Use plural (e.g., `Transactions`, `Splits`).
- **Commands**: Verb-based names (e.g., `Create`, `Update`, `Delete`, `Close`).
- **Queries**: Descriptive names (e.g., `Get`, `GetAll`, `GetForMonth`).
- **DTOs/Models**: Match entity names but in the Models namespace.

### File Organization
- Group related files by feature/entity, not by type.
- Commands, Queries, and Models specific to a feature should be in that feature's folder.
- Shared models belong in `Asm.MooBank.Models`.

### Error Handling
- Use `NotFoundException` when a requested resource doesn't exist.
- Use `BadHttpRequestException` for invalid request data.
- Use `UnauthorizedAccessException` or authorization policies for security violations.
- Domain validation should throw domain-specific exceptions.

### API Design
- Use RESTful conventions for endpoint URLs.
- Group endpoints logically (e.g., `/api/accounts/{id}/virtual`, `/api/instruments/{id}/import`).
- Use proper HTTP verbs (GET, POST, PATCH, DELETE).
- Return appropriate status codes.
- Include OpenAPI documentation attributes.

### Database Migrations
- Database schema is defined in the Database Project.
- Changes should be made to SQL files, not generated via EF migrations.
- Use database project deployment for schema updates.

# Important Technical Notes

## OpenAPI Generation
- The project uses `Microsoft.AspNetCore.OpenApi` (not Swashbuckle) for OpenAPI document generation.
- OpenAPI documents are generated at build time via `Microsoft.Extensions.ApiDescription.Server`.
- **Do not add `Swashbuckle.AspNetCore` as it conflicts with build-time generation**.
- Security schemes (OIDC) must handle null configuration gracefully for build-time generation.

## Module Registration
- Modules must implement `IModule` from `Asm.AspNetCore.Modules`.
- Register in `Program.cs` via `builder.RegisterModules()`.
- Each module's `AddServices()` method registers its dependencies.
- Each module's `MapEndpoints()` method defines its API endpoints.

## Data Access
- Always use the Unit of Work pattern for transactions.
- Repositories should only be used for aggregate roots.
- Use specifications for complex queries.
- Navigation properties should be explicitly loaded via specifications.
- Avoid N+1 query problems by using `.Include()` or specifications.

## Domain Events
- Raised within aggregate roots during state changes.
- Handled by event handlers in the infrastructure layer.
- Used for decoupled communication between aggregates.
- Example: `TransactionAddedEvent` triggers balance updates.

# Instructions for AI Agents

When contributing to the MooBank codebase:

1. **Read the AGENTS.md file first** to understand the architecture and patterns.
2. **Follow the established patterns**: CQRS, DDD, specifications, modules.
3. **Respect the folder structure**: Place files in the correct project and folder.
4. **Use the ASM library conventions**: Commands, Queries, and their handlers.
5. **Maintain separation of concerns**: Domain logic in Domain, infrastructure in Infrastructure, API concerns in Web.Api.
6. **Write clear, descriptive names** for classes, methods, and variables.
7. **Use domain-driven language** that matches the business concepts.
8. **Add appropriate tests** for new features (BDD tests for features, unit tests for complex logic).
9. **Follow C# coding conventions** as defined in `.editorconfig`.
10. **Document API endpoints** with OpenAPI attributes.
11. **Handle errors appropriately** using domain exceptions and HTTP exceptions.
12. **Consider security** and apply authorization policies where appropriate.
13. **Think about the full stack**: Consider both backend and frontend implications.
14. **Avoid breaking changes**: Maintain backward compatibility in APIs when possible.
15. **Use TypeScript strictly** in the frontend for type safety.

When making changes:
- Search for similar existing patterns before implementing new ones.
- Check for existing specifications, commands, or queries that might be reusable.
- Consider the impact on both the API and the frontend.
- Update tests to cover your changes.
- Ensure migrations or database changes are included if needed.

When in doubt:
- Look for similar examples in the codebase.
- Consult the ASM library documentation: https://github.com/AndrewMcLachlan/ASM
- Consult the MooApp library documentation: https://github.com/AndrewMcLachlan/MooApp
- Ask clarifying questions before making assumptions.
