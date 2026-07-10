---
paths:
  - "src/MooBank.Web.App/**/*.{ts,tsx}"
---

# TypeScript / React Conventions

**Full frontend conventions live in `src/MooBank.Web.App/CLAUDE.md` — read it before writing frontend code.** This rule covers only the essentials.

## Technology Stack

- **React 19** - UI framework
- **TypeScript** - `strictNullChecks: false` and `noImplicitAny: false` are accepted trade-offs for TanStack Router typing — do not "fix" them
- **Vite 7** - Build tool and dev server
- **TanStack Router** - File-based routing (`src/routes/`, generated `src/routeTree.gen.ts`)
- **TanStack Query v5** - Server state management
- **MooApp / MooDS** - Application framework and component library (Storybook: https://storybook.mclachlan.family; source: https://github.com/AndrewMcLachlan/MooApp)
- **MSAL** - Microsoft Authentication Library for Azure AD

## Type Safety

- All components and functions should have proper type annotations
- Avoid `any` where practical - prefer proper typing or `unknown` with type guards
- `src/api/**` and `src/routeTree.gen.ts` are generated — never hand-edit

## API Integration

- All API access goes through the generated `@hey-api/openapi-ts` client (`src/api/`) and its generated TanStack Query options — there are no hand-coded HTTP services
- Run `npm run generate` to regenerate API types after backend changes (the OpenAPI spec regenerates on `dotnet build`)

## Project Structure

```
src/MooBank.Web.App/src/
├── api/            # GENERATED API client + types
├── routes/         # File-based routes with co-located -components/ -hooks/ -utils/
├── components/     # Cross-feature shared components
├── hooks/          # Cross-feature shared hooks
├── utils/          # Shared utilities
└── routeTree.gen.ts  # GENERATED route tree
```
