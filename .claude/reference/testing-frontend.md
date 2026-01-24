# Frontend Testing Guidelines

## Overview

Frontend testing uses two approaches:
- **Component Tests** - Shared React components using Vitest
- **E2E Tests** - User workflow testing with Playwright

## React Component Tests

### Scope
**Only test shared components** in `src/components/` that are used across multiple pages. Do not write tests for page-specific components.

### Location
```
src/Asm.MooBank.Web.App/src/components/__tests__/
```

### Framework
- **Vitest** - Test runner (Vite-native)
- **React Testing Library** - Component testing

### What to Test
- ✅ Shared components in `src/components/`
- ✅ Custom hooks in `src/hooks/` with complex logic
- ✅ Utility functions in `src/helpers/`

### What NOT to Test
- ❌ Page components (`src/pages/*`)
- ❌ Simple wrapper components
- ❌ Components only used in one place

### Example Component Test
```typescript
// src/components/__tests__/AmountDisplay.test.tsx
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { AmountDisplay } from '../AmountDisplay';

describe('AmountDisplay', () => {
    it('renders positive amounts without negative class', () => {
        render(<AmountDisplay amount={100.50} />);

        const element = screen.getByText('$100.50');
        expect(element).not.toHaveClass('negative');
    });

    it('renders negative amounts with negative class', () => {
        render(<AmountDisplay amount={-50.25} />);

        const element = screen.getByText('-$50.25');
        expect(element).toHaveClass('negative');
    });

    it('formats currency correctly', () => {
        render(<AmountDisplay amount={1234.5} />);

        expect(screen.getByText('$1,234.50')).toBeInTheDocument();
    });
});
```

### Example Hook Test
```typescript
// src/hooks/__tests__/useDebounce.test.ts
import { renderHook, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useDebounce } from '../useDebounce';

describe('useDebounce', () => {
    beforeEach(() => {
        vi.useFakeTimers();
    });

    afterEach(() => {
        vi.useRealTimers();
    });

    it('returns initial value immediately', () => {
        const { result } = renderHook(() => useDebounce('initial', 500));
        expect(result.current).toBe('initial');
    });

    it('updates value after delay', () => {
        const { result, rerender } = renderHook(
            ({ value }) => useDebounce(value, 500),
            { initialProps: { value: 'initial' } }
        );

        rerender({ value: 'updated' });
        expect(result.current).toBe('initial');

        act(() => {
            vi.advanceTimersByTime(500);
        });

        expect(result.current).toBe('updated');
    });
});
```

### Running Component Tests
```bash
cd src/Asm.MooBank.Web.App

# Run tests
npm test

# Run with coverage
npm test -- --coverage

# Run in watch mode
npm test -- --watch

# Run specific test file
npm test -- AmountDisplay
```

## Playwright E2E Tests

### Location
```
tests/e2e/
```

### Framework
- **Playwright** - Cross-browser E2E testing

### What to Test
- Critical user workflows (happy paths)
- Authentication flow
- Core features: account creation, transaction import, tagging, budgeting
- Error states for important flows

### Test Structure
```
tests/e2e/
├── fixtures/
│   └── test-data.ts       # Shared test data
├── pages/
│   ├── dashboard.page.ts  # Page object for dashboard
│   ├── accounts.page.ts   # Page object for accounts
│   └── login.page.ts      # Page object for login
├── tests/
│   ├── auth.spec.ts       # Authentication tests
│   ├── accounts.spec.ts   # Account management tests
│   ├── transactions.spec.ts
│   └── budgets.spec.ts
└── playwright.config.ts
```

### Page Object Pattern
```typescript
// tests/e2e/pages/accounts.page.ts
import { Page, Locator } from '@playwright/test';

export class AccountsPage {
    readonly page: Page;
    readonly createButton: Locator;
    readonly accountNameInput: Locator;
    readonly saveButton: Locator;

    constructor(page: Page) {
        this.page = page;
        this.createButton = page.getByRole('button', { name: 'Create Account' });
        this.accountNameInput = page.getByLabel('Account Name');
        this.saveButton = page.getByRole('button', { name: 'Save' });
    }

    async goto() {
        await this.page.goto('/accounts');
    }

    async createAccount(name: string) {
        await this.createButton.click();
        await this.accountNameInput.fill(name);
        await this.saveButton.click();
    }

    async expectAccountVisible(name: string) {
        await expect(this.page.getByText(name)).toBeVisible();
    }
}
```

### Example E2E Tests
```typescript
// tests/e2e/tests/accounts.spec.ts
import { test, expect } from '@playwright/test';
import { AccountsPage } from '../pages/accounts.page';

test.describe('Account Management', () => {
    test('user can create a new account', async ({ page }) => {
        const accountsPage = new AccountsPage(page);

        await accountsPage.goto();
        await accountsPage.createAccount('My Savings Account');

        await accountsPage.expectAccountVisible('My Savings Account');
    });

    test('user can view account transactions', async ({ page }) => {
        const accountsPage = new AccountsPage(page);

        await accountsPage.goto();
        await page.getByText('Existing Account').click();

        await expect(page.getByRole('table')).toBeVisible();
        await expect(page.getByText('Transactions')).toBeVisible();
    });
});
```

```typescript
// tests/e2e/tests/transactions.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Transaction Import', () => {
    test('user can import transactions from CSV', async ({ page }) => {
        await page.goto('/accounts/123');

        await page.getByRole('button', { name: 'Import' }).click();

        const fileInput = page.locator('input[type="file"]');
        await fileInput.setInputFiles('tests/e2e/fixtures/sample-transactions.csv');

        await page.getByRole('button', { name: 'Upload' }).click();

        await expect(page.getByText('45 transactions imported')).toBeVisible();
    });
});
```

### Playwright Configuration
```typescript
// tests/e2e/playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: './tests',
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    workers: process.env.CI ? 1 : undefined,
    reporter: 'html',
    use: {
        baseURL: 'http://localhost:3005',
        trace: 'on-first-retry',
    },
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
    ],
    webServer: {
        command: 'npm run start',
        url: 'http://localhost:3005',
        reuseExistingServer: !process.env.CI,
        cwd: '../src/Asm.MooBank.Web.App',
    },
});
```

### Running E2E Tests
```bash
# Install Playwright browsers (first time)
npx playwright install

# Run all E2E tests
npx playwright test

# Run specific test file
npx playwright test tests/e2e/tests/accounts.spec.ts

# Run in headed mode (see browser)
npx playwright test --headed

# Run in UI mode (interactive)
npx playwright test --ui

# Generate test report
npx playwright show-report

# Debug a specific test
npx playwright test --debug
```

## Test Distribution

```
Component Tests
├── Shared components only
├── Complex custom hooks
└── Utility functions

E2E Tests
├── Authentication flow
├── Account management
├── Transaction import/tagging
├── Budget creation/tracking
└── Critical error states
```

## CI Integration

```yaml
# .github/workflows/test.yml
- name: Run Frontend Component Tests
  working-directory: src/Asm.MooBank.Web.App
  run: npm test -- --coverage

- name: Install Playwright Browsers
  run: npx playwright install --with-deps

- name: Run E2E Tests
  run: npx playwright test

- name: Upload Playwright Report
  uses: actions/upload-artifact@v4
  if: always()
  with:
    name: playwright-report
    path: playwright-report/
```
