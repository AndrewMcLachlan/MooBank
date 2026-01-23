# Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

See `.claude/PRD.md` for detailed product requirements and domain concepts.

# Architecture

## Technology Stack

The technology stack for MooBank includes:
- **Frontend**: React.js (TypeScript) for building a responsive and interactive user interface.
- **Backend**: .NET / ASP.NET Core. MooBank uses the latest general-availability version of .NET (including STS versions).
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

See `.claude/reference/cqrs.md` and `.claude/reference/entity-framework.md` for implementation details.

## Code Layout (Backend)

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
- **MooApp**: A set of custom packages of React components and hooks to simplify application development.
    - **@andrewmclachlan/moo-ds** - An opinionated component library, based on Bootstrap and React-Bootstrap
    - **@andrewmclachlan/moo-app** - Provides app-level components such as:
        - An application bootstrapper
        - A layout
        - Authentication with MSAL
        - API management with React Query and Axios, with custom React Query hooks for different HTTP verbs


The code for MooApp is available at: https://github.com/AndrewMcLachlan/MooApp

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
    - **Use Framework types correctly** (e.g. `string` for declarations and `String` for static methods).
10. **No Warnings**: Ensure code compiles without warnings.
11. **Document API endpoints** with OpenAPI attributes.
12. **Handle errors appropriately** using domain exceptions and HTTP exceptions.
13. **Consider security** and apply authorization policies where appropriate.
14. **Think about the full stack**: Consider both backend and frontend implications.
15. **Avoid breaking changes**: Maintain backward compatibility in APIs when possible.
16. **Use TypeScript strictly** in the frontend for type safety.

When making changes:
- Search for similar existing patterns before implementing new ones.
- Check for existing specifications, commands, or queries that might be reusable.
- Consider the impact on both the API and the frontend.
- Update tests to cover your changes.
- Ensure migrations or database changes are included if needed.

When in doubt:
- Look for similar examples in the codebase.
- Consult the ASM library source code: https://github.com/AndrewMcLachlan/ASM
- Consult the MooApp library source code: https://github.com/AndrewMcLachlan/MooApp
- Consult the MooApp UI component library Storybook: https://storybook.mclachlan.family
- Ask clarifying questions before making assumptions.

# Technology References

For detailed implementation guidelines, see:
- `.claude/reference/csharp.md` - C# coding conventions, naming, error handling
- `.claude/reference/typescript.md` - TypeScript/React frontend conventions
- `.claude/reference/sql-database.md` - Database design and migrations
- `.claude/reference/rest-api.md` - REST API design and authorization
- `.claude/reference/entity-framework.md` - EF Core data access patterns
- `.claude/reference/cqrs.md` - CQRS commands, queries, and endpoints
