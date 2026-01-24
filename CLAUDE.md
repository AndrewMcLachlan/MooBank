# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the MooBank codebase.

## Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

See `.claude/PRD.md` for detailed product requirements and domain concepts.

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
├── Asm.MooBank.Web.Api/           # ASP.NET Core Web API (minimal, delegates to modules)
├── Asm.MooBank.Domain/            # Domain entities, aggregates, specifications, events
├── Asm.MooBank.Infrastructure/    # EF Core, repository implementations
├── Asm.MooBank.Models/            # Shared DTOs
├── Asm.MooBank.Modules.*/         # Feature modules (CQRS commands/queries/endpoints)
├── Asm.MooBank.Security/          # Auth policies
├── Asm.MooBank.Institution.*/     # Bank importers (ING, Macquarie, AustralianSuper)
├── Asm.MooBank.Web.App/           # React/TypeScript SPA
├── Asm.MooBank.Web.Jobs/          # Azure WebJobs (background processing)
├── Asm.MooBank.Database/          # SQL Server Database Project
└── Asm.MooBank.AppHost/           # Aspire AppHost for local dev
tests/
└── Asm.MooBank.Tests/             # Unit and integration tests
```

### Libraries and Frameworks

**Backend:**
- **ASM Library** (https://github.com/AndrewMcLachlan/ASM) - Custom NuGet packages:
  - `Asm.AspNetCore.Api` - OpenAPI configuration
  - `Asm.AspNetCore.Modules` - Module registration and endpoint mapping
  - `Asm.Cqrs.AspNetCore` - CQRS endpoint extensions
  - `Asm.Domain` / `Asm.Domain.Infrastructure` - DDD base classes
- **ReqnRoll** - BDD testing framework

**Frontend:**
- **MooApp** (https://github.com/AndrewMcLachlan/MooApp) - Custom React packages:
  - `@andrewmclachlan/moo-ds` - Component library (Storybook: https://storybook.mclachlan.family)
  - `@andrewmclachlan/moo-app` - App framework with MSAL auth, React Query hooks

## Claude Code Commands

| Command | Description |
|---------|-------------|
| `/project:core-piv/prime` | Load project context before implementation |
| `/project:core-piv/plan-feature <desc>` | Create implementation plan for a feature |
| `/project:core-piv/execute <plan-file>` | Execute a plan step-by-step |
| `/project:validation/validate` | Run full validation suite |
| `/project:validation/code-review` | Review changed files |
| `/project:validation/test` | Run test suite |
| `/project:commit` | Create conventional commit |
| `/project:init-project` | First-time project setup |

## Technology References

Read these when working on specific areas:

| Reference | Path | When to Read |
|-----------|------|--------------|
| C# | `.claude/reference/csharp.md` | Backend code, domain entities, handlers |
| TypeScript | `.claude/reference/typescript.md` | React frontend |
| SQL / Database | `.claude/reference/sql-database.md` | Schema changes, migrations |
| REST API | `.claude/reference/rest-api.md` | Endpoints, authorization, OpenAPI |
| Entity Framework | `.claude/reference/entity-framework.md` | Data access, repositories, specifications |
| CQRS | `.claude/reference/cqrs.md` | Commands, queries, endpoint mappings |
| Backend Testing | `.claude/reference/testing-backend.md` | ReqnRoll unit tests, authorization integration tests |
| Frontend Testing | `.claude/reference/testing-frontend.md` | Vitest component tests, Playwright E2E tests |

## Instructions for AI Agents

### Core Principles

1. **Follow established patterns**: CQRS, DDD, specifications, modules
2. **Respect folder structure**: Place files in the correct project and folder
3. **Use ASM library conventions**: Commands, Queries, and their handlers
4. **Maintain separation of concerns**: Domain → Infrastructure → Web.Api
5. **Write clear, descriptive names** using domain-driven language
6. **No warnings**: Ensure code compiles without warnings
7. **Consider security**: Apply authorization policies where appropriate
8. **Think full stack**: Consider both API and frontend implications

### Coding Standards

- Follow C# conventions in `.editorconfig`
- Use `string` for declarations, `String` for static methods
- Use TypeScript strictly in the frontend
- Handle errors with domain exceptions and HTTP exceptions
- Document API endpoints with OpenAPI attributes

### When Making Changes

- Search for similar existing patterns first
- Check for reusable specifications, commands, or queries
- Consider impact on both API and frontend
- Update tests to cover changes
- Include database changes if needed

### When in Doubt

- Look for similar examples in the codebase
- Consult ASM library: https://github.com/AndrewMcLachlan/ASM
- Consult MooApp library: https://github.com/AndrewMcLachlan/MooApp
- Consult MooDS Storybook: https://storybook.mclachlan.family
- Ask clarifying questions before making assumptions

## Additional Resources

- `.claude/PRD.md` - Product requirements and domain overview
- `.agents/plans/` - Feature implementation plans
