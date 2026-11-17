# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the MooBank codebase.

## Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

Product requirements and domain concepts are in `.claude/PRD.md`; read it when a task needs product context.

## Architecture

### Technology Stack

- **Frontend**: React.js (TypeScript) for building a responsive and interactive user interface.
- **Backend**: .NET / ASP.NET Core. MooBank uses the latest general-availability version of .NET (including STS versions).
- **Database**: Azure SQL Server with Entity Framework Core as the ORM.
- **API Documentation**: Microsoft.AspNetCore.OpenApi (not Swashbuckle) with Swagger UI for interactive documentation.

### Software Design Patterns

- **Minimal API** - Endpoints are defined using ASP.NET Core Minimal APIs.
- **Command Query Responsibility Segregation (CQRS)** - Commands (write operations) and Queries (read operations) are separated.
- **Domain-Driven Design (DDD)** - Business logic is encapsulated in domain entities with rich behavior.
- **Unit of Work and Repository Patterns** - For managing database transactions and data access.
- **Specification Pattern** - Used for composable query logic in repositories.
- **Domain Events** - For decoupled communication between aggregates.

### Code Layout

```
src/
├── MooBank.Api/               # ASP.NET Core Web API entry point (also hosts Azure WebJobs under Jobs/)
├── MooBank/                   # Application core: cross-cutting abstractions, application services, hosted workers, entity↔DTO mapping (one project by design)
├── MooBank.Domain/            # Domain entities, aggregates, specifications, events
├── MooBank.Infrastructure/    # EF Core, repository implementations
├── MooBank.Models/            # Shared DTOs
├── MooBank.Modules.*/         # Feature modules (CQRS commands/queries/endpoints)
├── MooBank.Security/          # Auth policies and requirement handlers
├── MooBank.Institution.*/     # Bank importers (ING, Macquarie, AustralianSuper)
├── MooBank.Abs / .Eodhd / .ExchangeRateApi  # Reference-data integration clients
├── MooBank.Web.App/           # React/TypeScript SPA (see src/MooBank.Web.App/CLAUDE.md)
├── MooBank.Database/          # SQL Server Database Project
└── MooBank.AppHost/           # Aspire AppHost for local dev
tests/
└── MooBank.*.Tests/           # Unit and integration tests
```

Note: projects live in `MooBank.*` folders but build as `Asm.MooBank.*` assemblies/namespaces (see `Directory.Build.props`).

### Libraries and Frameworks

**Backend:**
- **ASM Library** (https://github.com/AndrewMcLachlan/ASM) - Custom NuGet packages:
  - `Asm.AspNetCore.Api` - OpenAPI configuration
  - `Asm.AspNetCore.Modules` - Module registration and endpoint mapping
  - `Postie.Cqrs.AspNetCore` / `Postie.AspNetCore` - CQRS endpoint extensions
  - `Asm.Domain` / `Asm.Domain.Infrastructure` - DDD base classes
- **ReqnRoll** - BDD testing framework

**Frontend:**
- **MooApp** (https://github.com/AndrewMcLachlan/MooApp) - Custom React packages:
  - `@andrewmclachlan/moo-ds` - Component library (Storybook: https://storybook.mclachlan.family)
  - `@andrewmclachlan/moo-app` - App framework with MSAL auth, React Query hooks

## Build & Test Commands

```bash
# Backend
dotnet build MooBank.slnx                   # Build solution
dotnet test tests/                          # Run all tests
dotnet test --filter /[Category=Unit]       # Unit tests only

# Frontend (from src/MooBank.Web.App/)
npm run build                               # Build (includes tsc type checking)
npm run lint                                # Lint
npm run generate                            # Regenerate API types from OpenAPI spec
npm test                                    # Run Vitest component tests
```

## Instructions for AI Agents

### Core Principles

1. **Use ASM library conventions**: Commands, Queries, and their handlers
2. **No warnings**: Ensure code compiles without warnings
3. **Consider security**: Apply authorization policies where appropriate

### Coding Standards

- Follow C# conventions in `.editorconfig`
- Use `string` for declarations, `String` for static methods
- Use TypeScript strictly in the frontend
- Handle errors with domain exceptions and HTTP exceptions
- Document API endpoints with OpenAPI attributes
- Do not use `.WithOpenApi()` - it is deprecated in .NET 10

## Additional Resources

- `.agents/plans/` - Feature implementation plans
