---
paths:
  - "src/MooBank.Web.App/**/__tests__/**"
  - "src/MooBank.Web.App/**/*.test.{ts,tsx}"
  - "tests/e2e/**"
---

# Frontend Testing Guidelines

## Component / Unit Tests

### Harness
The Vitest + React Testing Library harness lives in `src/MooBank.Web.App`:
- `vitest.config.ts` — standalone config (jsdom, `globals: true`, `resolve.tsconfigPaths` for bare
  imports). It deliberately does **not** import `vite.config.ts`, whose top-level code generates
  dev certificates via `dotnet dev-certs` (unavailable in CI).
- `src/test/setup.ts` — registers `@testing-library/jest-dom` and cleans up after each test.
- `src/test/matchers.d.ts` — makes the jest-dom matcher types visible to `tsgo`.
- Scripts: `npm test` (`vitest run`), `npm run test:watch`, `npm run test:coverage`.
- Test files (`*.test.ts[x]`) stay under `src/` and are type-checked by the production `tsgo` build,
  so they must remain type-correct.

### Framework
- **Vitest** - Test runner (Vite-native)
- **React Testing Library** - Component testing

### Scope — test by value, not by folder
Write a test when it covers **genuinely high-value logic** and gives a fast, deterministic signal.
That value, not a component's location, decides whether it is worth testing.

### What to Test
- Shared components in `src/components/` and hooks in `src/hooks/` with real logic
- Utility functions in `src/utils/` (`currency.ts`, date helpers, key builders)
- **High-value route-level logic** — a route component or co-located `-hooks/`/`-components/` file is
  fair game when it carries real branching worth pinning. Examples that exist today:
  `routes/accounts/-transactions/components/AddTransaction.tsx` (amount vs new-balance routing,
  blank→undefined, Save enablement) and `routes/accounts/-hooks/transactionKeys.ts` (partial-key
  matching). Mock the generated `@hey-api`/React Query hooks; provide context via the real providers.

### What NOT to Test
- Trivial wrapper components with no branching
- Generated code (`src/api/**`, `src/routeTree.gen.ts`)
- Purely presentational markup with no logic

### Running Component Tests
```bash
cd src/MooBank.Web.App

# Run tests
npm test

# Run with coverage
npm test -- --coverage

# Run specific test file
npm test -- AmountDisplay
```

## Playwright E2E Tests

### Location
```
tests/e2e/
```

### What to Test
- Critical user workflows (happy paths)
- Authentication flow
- Core features: account creation, transaction import, tagging, budgeting
- Error states for important flows

### Test Structure - Use Page Object Pattern
```
tests/e2e/
├── fixtures/           # Shared test data
├── pages/              # Page objects
├── tests/              # Test specs
└── playwright.config.ts
```

### Running E2E Tests
```bash
npx playwright test
npx playwright test --headed
npx playwright test --ui
```
