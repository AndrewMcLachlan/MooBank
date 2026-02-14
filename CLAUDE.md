# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the MooBank codebase.

## Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

See @.claude/PRD.md for detailed product requirements and domain concepts.

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

## Build & Test Commands

```bash
# Backend
dotnet build Asm.MooBank.slnx              # Build solution
dotnet test tests/                          # Run all tests
dotnet test --filter /[Category=Unit]       # Unit tests only

# Frontend (from src/Asm.MooBank.Web.App/)
npm run build                               # Build (includes tsc type checking)
npm run lint                                # Lint
npm run generate                            # Regenerate API types from OpenAPI spec
npm test                                    # Run Vitest component tests
```

## Skills (Slash Commands)

### PIV Loop (Plan → Implement → Validate)

| Command | Description |
|---------|-------------|
| `/plan-feature <description>` | Create implementation plan (runs in isolated subagent) |
| `/execute <plan-file>` | Execute a plan step-by-step |
| `/validate` | Run full validation suite (build, lint, test) |
| `/code-review` | Review changed files for bugs and quality |
| `/code-review-fix <review-file>` | Fix issues found in a code review |
| `/review-implementation <plan-file>` | Analyze implementation vs plan, suggest process improvements |

### Utilities

| Command | Description |
|---------|-------------|
| `/commit` | Create conventional commit |
| `/test` | Run test suite |
| `/init-project` | First-time project setup |
| `/create-prd <filename>` | Generate Product Requirements Document |

## Technology Rules

Path-scoped rules in `.claude/rules/` auto-load when working on matching files:

| Rule | Auto-loads for |
|------|---------------|
| `backend/csharp.md` | `src/**/*.cs` |
| `backend/cqrs.md` | `src/Asm.MooBank.Modules*/**` |
| `backend/entity-framework.md` | `src/Asm.MooBank.Infrastructure/**`, `src/Asm.MooBank.Domain/**` |
| `backend/rest-api.md` | `src/Asm.MooBank.Web.Api/**`, `src/Asm.MooBank.Modules*/Endpoints/**` |
| `backend/sql-database.md` | `src/Asm.MooBank.Database/**` |
| `frontend/typescript.md` | `src/Asm.MooBank.Web.App/**/*.{ts,tsx}` |
| `testing/backend.md` | `tests/**/*.cs` |
| `testing/frontend.md` | `tests/e2e/**`, `src/Asm.MooBank.Web.App/**/__tests__/**` |

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
- Do not use `.WithOpenApi()` - it is deprecated in .NET 10

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
