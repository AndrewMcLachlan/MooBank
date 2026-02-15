---
paths:
  - "src/Asm.MooBank.Web.App/**/*.{ts,tsx}"
---

# TypeScript / React Conventions

## Technology Stack

- **React 19** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool and dev server
- **React Router 7** - Client-side routing
- **React Query (TanStack Query v5)** - Server state management
- **MooApp** - Application framework, including UI components
- **React Bootstrap** - UI component library
- **MSAL** - Microsoft Authentication Library for Azure AD

## Custom Libraries

### MooApp
A set of custom packages for React development:

- **@andrewmclachlan/moo-ds** - An opinionated component library inspired by, but not dependent on, Bootstrap and React-Bootstrap
- **@andrewmclachlan/moo-app** - Provides app-level components:
  - Application bootstrapper
  - Layout components
  - Authentication with MSAL
  - API management with React Query and Axios
  - Custom React Query hooks for different HTTP verbs

Source Code: https://github.com/AndrewMcLachlan/MooApp
Storybook (for moo-ds): https://storybook.mclachlan.family

## Type Safety

- Use TypeScript strictly for type safety
- All components and functions should have proper type annotations
- Avoid `any` type - use proper typing or `unknown` with type guards

## API Integration

- Use React Query for all API calls
- API types are generated from OpenAPI spec via `@hey-api/openapi-ts`
- Run `npm run generate` to regenerate API types after backend changes

## Project Structure

```
src/Asm.MooBank.Web.App/
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/          # Route-level components
│   ├── hooks/          # Custom React hooks
│   ├── services/       # API service functions
│   └── types/          # TypeScript type definitions
├── public/             # Static assets
└── index.html          # Entry HTML file
```
