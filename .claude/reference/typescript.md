# TypeScript / React Conventions

## Overview

The frontend is a React 19 single-page application using TypeScript, located in `src/Asm.MooBank.Web.App`.

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

- **@andrewmclachlan/moo-ds** - An opinionated component library based on Bootstrap and React-Bootstrap
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

## Build Commands

```bash
# Development server
npm run start

# Production build
npm run build

# Lint code
npm run lint

# Fix lint errors
npm run lint-fix

# Regenerate API types
npm run generate
```

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
