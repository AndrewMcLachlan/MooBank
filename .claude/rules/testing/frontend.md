---
paths:
  - "src/Asm.MooBank.Web.App/**/__tests__/**"
  - "src/Asm.MooBank.Web.App/**/*.test.{ts,tsx}"
  - "tests/e2e/**"
---

# Frontend Testing Guidelines

## Component Tests

### Scope
**Only test shared components** in `src/components/` that are used across multiple pages. Do not write tests for page-specific components.

### Framework
- **Vitest** - Test runner (Vite-native)
- **React Testing Library** - Component testing

### What to Test
- Shared components in `src/components/`
- Custom hooks in `src/hooks/` with complex logic
- Utility functions in `src/helpers/`

### What NOT to Test
- Page components (`src/pages/*`)
- Simple wrapper components
- Components only used in one place

### Running Component Tests
```bash
cd src/Asm.MooBank.Web.App

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
