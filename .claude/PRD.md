# Product Requirements Document - MooBank

## 1. Executive Summary

MooBank is a browser-based personal finance management application designed to help individuals track, manage, and analyze their financial activities effectively. The application provides a unified view of all financial instruments—bank accounts, credit cards, loans, investments, and superannuation—enabling users to understand their complete financial picture.

The core value proposition is **financial clarity through consolidation**: users can import transactions from multiple financial institutions, categorize spending with a flexible tagging system, track budgets, and gain insights through reporting and forecasting tools.

MooBank is a mature application with an established feature set. The current MVP goal is maintaining feature parity while modernizing the codebase and improving developer experience through better architecture and tooling.

## 2. Mission

**Mission Statement:** Empower individuals to take control of their personal finances through intuitive tools that consolidate, categorize, and analyze financial data from multiple sources.

**Core Principles:**
1. **Consolidation** - Bring all financial data into one place
2. **Flexibility** - Support diverse financial products and categorization needs
3. **Privacy** - User data stays under user control (no third-party data sharing)
4. **Simplicity** - Complex financial data presented in understandable ways
5. **Automation** - Reduce manual effort through import tools and recurring transactions

## 3. Target Users

**Primary Persona: The Financially Aware Individual**
- Adults managing personal/household finances
- Comfortable with basic technology (web applications, file uploads)
- Has accounts at multiple financial institutions
- Wants visibility into spending patterns and budget adherence
- Values data ownership over convenience of third-party aggregators

**Technical Comfort Level:** Moderate
- Can navigate web applications confidently
- Comfortable downloading CSV files from bank websites
- May not be technically sophisticated (no API knowledge expected)

**Key Pain Points:**
- Financial data scattered across multiple institution portals
- Difficulty understanding where money is going
- Manual spreadsheet tracking is tedious and error-prone
- Existing apps require sharing bank credentials (security concern)

## 4. MVP Scope

### In Scope - Core Functionality
- ✅ Multi-instrument dashboard with consolidated balances
- ✅ Transaction import via CSV from supported institutions
- ✅ Transaction tagging and categorization
- ✅ Transaction splits across multiple categories
- ✅ Budget creation and tracking
- ✅ Spending reports and trends
- ✅ Recurring transaction scheduling (Virtual Instruments)
- ✅ Instrument grouping for aggregate views

### In Scope - Technical
- ✅ Azure AD authentication (OAuth 2.0 / OIDC)
- ✅ Multi-tenancy via Family groupings
- ✅ RESTful API with OpenAPI documentation
- ✅ Background job processing for scheduled tasks
- ✅ Reference data integration (exchange rates, inflation, stock prices)

### In Scope - Deployment
- ✅ Azure App Service hosting
- ✅ Azure SQL Database
- ✅ GitHub Actions CI/CD
- ✅ Application Insights monitoring

### Out of Scope (Future Phases)
- ❌ Direct bank API integrations (Open Banking)
- ❌ Mobile native applications
- ❌ Multi-currency portfolio rebalancing
- ❌ Tax reporting/optimization features
- ❌ Bill payment functionality
- ❌ Shared expense splitting between users

## 5. User Stories

### Primary User Stories

1. **As a user, I want to see all my account balances in one dashboard**, so that I understand my total financial position at a glance.
   - *Example: Dashboard shows savings ($15,000), credit card (-$2,500), mortgage (-$350,000), shares ($45,000)*

2. **As a user, I want to import transactions from my bank's CSV export**, so that I don't have to manually enter each transaction.
   - *Example: Upload ING CSV file, system parses and creates transactions with dates, amounts, and descriptions*

3. **As a user, I want to tag transactions with categories**, so that I can track spending by category.
   - *Example: Tag "Woolworths" transaction as "Groceries", "Amazon" as "Shopping"*

4. **As a user, I want to split a single transaction across multiple categories**, so that I can accurately categorize mixed purchases.
   - *Example: $150 Costco purchase split: $100 Groceries, $30 Household, $20 Entertainment*

5. **As a user, I want to create budgets for spending categories**, so that I can control my spending.
   - *Example: Set $600/month budget for Groceries, see progress bar showing $450 spent*

6. **As a user, I want to see spending trends over time**, so that I can identify patterns and adjust behavior.
   - *Example: Chart showing grocery spending increased 15% over last 6 months*

7. **As a user, I want to schedule recurring transactions**, so that I can track expected income/expenses on virtual accounts.
   - *Example: $2,000 salary every 2 weeks, $150 electricity quarterly*

8. **As a user, I want to group related accounts together**, so that I can see combined balances.
   - *Example: "Emergency Fund" group containing 3 savings accounts totaling $25,000*

### Technical User Stories

9. **As a developer, I want to add new institution importers**, so that users can import from additional banks.
   - *Example: Implement `IImporter` for Westpac CSV format*

10. **As an operator, I want background jobs to update reference data automatically**, so that exchange rates and stock prices stay current.
    - *Example: Nightly job fetches AUD/USD rate from ExchangeRateApi*

## 6. Core Architecture & Patterns

### High-Level Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   React SPA     │────▶│   ASP.NET Core  │────▶│   Azure SQL     │
│   (TypeScript)  │     │   Minimal API   │     │   Database      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  Azure WebJobs  │
                        │  (Background)   │
                        └─────────────────┘
```

### Design Patterns

- **CQRS** - Commands (writes) and Queries (reads) are separated
- **Domain-Driven Design** - Rich domain entities with business logic
- **Repository Pattern** - Aggregate root access via repositories
- **Specification Pattern** - Composable query logic
- **Unit of Work** - Transaction management
- **Domain Events** - Decoupled aggregate communication

### Directory Structure

```
src/
├── Asm.MooBank.Web.Api/           # API entry point (minimal)
├── Asm.MooBank.Domain/            # Entities, aggregates, specifications
├── Asm.MooBank.Infrastructure/    # EF Core, repository implementations
├── Asm.MooBank.Modules.*/         # Feature modules (CQRS)
├── Asm.MooBank.Models/            # Shared DTOs
├── Asm.MooBank.Security/          # Auth policies
├── Asm.MooBank.Web.App/           # React frontend
├── Asm.MooBank.Web.Jobs/          # Background jobs
├── Asm.MooBank.Database/          # SQL database project
└── Asm.MooBank.Institution.*/     # Bank importers
```

### Module Structure

Each feature module follows:
```
Modules.{Feature}/
├── Commands/       # Write operations (ICommand, ICommandHandler)
├── Queries/        # Read operations (IQuery, IQueryHandler)
├── Models/         # Module-specific DTOs
├── Endpoints/      # Minimal API route definitions
└── Module.cs       # DI registration
```

## 7. Features

### Instrument Management
- Create/edit/delete financial instruments
- Support for: Bank Accounts, Credit Cards, Loans, Mortgages, Shares, Superannuation
- Logical Instruments allow institution changes without losing history
- Virtual Instruments for tracking (budgets, savings goals)

### Transaction Management
- Import transactions via CSV upload
- Manual transaction entry
- Transaction tagging with hierarchical tags
- Transaction splits across multiple tags
- Bulk tagging operations
- Search and filter transactions

### Budgeting
- Create budgets by tag/category
- Monthly/yearly budget periods
- Budget vs actual tracking
- Rollover support

### Reporting
- Spending by category (pie/bar charts)
- Spending trends over time
- Net worth tracking
- Cash flow analysis
- Forecast projections

### Institution Importers
- **ING** - CSV import
- **Macquarie** - CSV import
- **AustralianSuper** - CSV import
- Raw transaction storage for reprocessing

### Reference Data
- Currency exchange rates (daily updates)
- Inflation data (ABS integration)
- Stock prices (EOD Historical Data)

## 8. Technology Stack

### Backend
| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 9.0+ (latest GA including STS) |
| Framework | ASP.NET Core Minimal APIs | 9.0+ |
| ORM | Entity Framework Core | 9.0+ |
| Database | Azure SQL Server | - |
| Background Jobs | Azure WebJobs | - |
| API Docs | Microsoft.AspNetCore.OpenApi | - |

### Frontend
| Component | Technology | Version |
|-----------|------------|---------|
| Framework | React | 19.x |
| Language | TypeScript | 5.x |
| Build Tool | Vite | 7.x |
| Routing | React Router | 7.x |
| State (Server) | TanStack Query | 5.x |
| State (UI) | Redux Toolkit | 2.x |
| UI Components | React Bootstrap | 2.x |
| Forms | React Hook Form | 7.x |
| Charts | Chart.js / react-chartjs-2 | 4.x |
| Dates | date-fns | 4.x |

### Custom Libraries
| Library | Purpose |
|---------|---------|
| ASM Library | Backend infrastructure (CQRS, DDD, modules) |
| MooApp (@andrewmclachlan/moo-app) | React app framework, API hooks |
| MooDS (@andrewmclachlan/moo-ds) | UI component library |

### Infrastructure
| Component | Service |
|-----------|---------|
| Hosting | Azure App Service |
| Database | Azure SQL Database |
| Background Jobs | Azure WebJobs |
| Secrets | Azure Key Vault |
| Monitoring | Application Insights |
| CI/CD | GitHub Actions |

## 9. Security & Configuration

### Authentication
- OAuth 2.0 / OpenID Connect via Azure AD
- MSAL (Microsoft Authentication Library) on frontend
- JWT token validation on backend

### Authorization
- Policy-based authorization
- Parameterized policies for instrument access
- Family-based multi-tenancy for data isolation

**Authorization Pattern:**
```csharp
// Always use parameterized policies for instrument endpoints
.RequireAuthorization(Policies.GetInstrumentViewerPolicy("instrumentId"));
.RequireAuthorization(Policies.GetInstrumentOwnerPolicy("instrumentId"));
```

### Configuration
- `appsettings.json` / `appsettings.Development.json` for app settings
- Azure Key Vault for production secrets
- Environment variables for deployment configuration

### Security Scope
**In Scope:**
- ✅ Azure AD authentication
- ✅ Role-based access control
- ✅ Family-based data isolation
- ✅ HTTPS enforcement
- ✅ Input validation

**Out of Scope:**
- ❌ End-to-end encryption of stored data
- ❌ Two-factor authentication (handled by Azure AD)
- ❌ Audit logging of all user actions

## 10. API Specification

### Base URL
```
https://moobank.example.com/api
```

### Key Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/accounts` | List all accounts |
| GET | `/accounts/{id}` | Get account details |
| POST | `/accounts` | Create account |
| PATCH | `/accounts/{id}` | Update account |
| DELETE | `/accounts/{id}` | Delete account |
| GET | `/instruments/{id}/transactions` | List transactions |
| POST | `/instruments/{id}/import` | Import CSV transactions |
| GET | `/tags` | List all tags |
| POST | `/transactions/{id}/tags` | Tag a transaction |
| GET | `/budgets` | List budgets |
| GET | `/reports/spending` | Spending report |

### Authentication
All endpoints require Bearer token:
```
Authorization: Bearer <jwt-token>
```

### Example Request
```http
POST /api/instruments/123/import
Content-Type: multipart/form-data
Authorization: Bearer eyJ...

file: transactions.csv
```

### Example Response
```json
{
  "imported": 45,
  "duplicates": 3,
  "errors": []
}
```

## 11. Success Criteria

### MVP Success Definition
The application successfully enables users to consolidate financial data, categorize transactions, and track budgets across multiple financial institutions.

### Functional Requirements
- ✅ Users can create and manage multiple instrument types
- ✅ Users can import transactions from supported institutions
- ✅ Users can tag and split transactions
- ✅ Users can create and track budgets
- ✅ Users can view spending reports and trends
- ✅ Background jobs update reference data automatically
- ✅ Multi-tenancy isolates family data

### Quality Indicators
- Page load time < 2 seconds
- API response time < 500ms (95th percentile)
- Zero security vulnerabilities (critical/high)
- Test coverage > 70%

### User Experience Goals
- Import workflow completes in < 5 clicks
- Dashboard provides at-a-glance financial summary
- Mobile-responsive design works on tablets

## 12. Implementation Phases

### Phase 1: Core Infrastructure (Complete)
**Goal:** Establish foundational architecture

**Deliverables:**
- ✅ Project structure with CQRS/DDD patterns
- ✅ Azure AD authentication
- ✅ Database schema and EF Core setup
- ✅ CI/CD pipeline

**Validation:** Application builds and deploys successfully

### Phase 2: Instrument & Transaction Management (Complete)
**Goal:** Core financial data management

**Deliverables:**
- ✅ Instrument CRUD operations
- ✅ Transaction management
- ✅ CSV import for major institutions
- ✅ Tagging system

**Validation:** Users can import and categorize transactions

### Phase 3: Budgeting & Reporting (Complete)
**Goal:** Financial insights and planning

**Deliverables:**
- ✅ Budget creation and tracking
- ✅ Spending reports
- ✅ Trend analysis
- ✅ Dashboard widgets

**Validation:** Users can create budgets and view reports

### Phase 4: Ongoing Maintenance
**Goal:** Stability and developer experience

**Deliverables:**
- ✅ Performance optimization
- ✅ Additional institution importers
- ✅ Enhanced reporting
- ✅ Developer tooling (Claude Code integration)

**Validation:** Improved development velocity, stable production

## 13. Future Considerations

### Product Features

- Smarter reporting
  - Showing reports that are only relevant for the instrument type (e.g. Transaction account vs savings account vs superannuation)
  - Creating reports that are specific to different types of instrument / account

- Improved visualisation of the tag graph

- Dashboard
   - More widgets on the dashboard
   - Dashboard is user-configurable

- Utility Bills
   - Link a bill to the corresponding transaction(S) for payment

- Budget
   - Smart generation of a month-by-month budget based on transaction history

### Technical Debt

- Testing. There are next to no back-end or front-end tests for MooBank.
- Front-end 
  - Replace hand-coded services with `ts-openapi` react-query hooks.
  - Architecture and code layout review



## 14. Risks & Mitigations

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Bank CSV format changes | Import failures | Medium | Store raw data for reprocessing; version importers |
| Azure AD outage | Users cannot login | Low | Graceful degradation; clear error messaging |
| Reference data API unavailable | Stale exchange rates | Medium | Cache last known values; background retry |
| Performance degradation with data growth | Slow queries | Medium | Pagination; query optimization; archival strategy |
| Security vulnerability in dependencies | Data breach | Low | Dependabot alerts; regular updates; security scanning |

## 15. Appendix

### Related Documents
- `AGENTS.md` - Developer coding conventions and patterns
- `.claude/reference/*.md` - Technology-specific guidelines

### Key Dependencies
| Dependency | Repository |
|------------|------------|
| ASM Library | https://github.com/AndrewMcLachlan/ASM |
| MooApp | https://github.com/AndrewMcLachlan/MooApp |
| MooDS Storybook | https://storybook.mclachlan.family |

### Repository Structure
```
MooBank/
├── src/                    # Source code
├── tests/                  # Test projects
├── .claude/                # Claude Code configuration
│   ├── commands/           # Slash commands
│   ├── reference/          # Technology guides
│   └── PRD.md              # This document
├── .agents/plans/          # Implementation plans
├── AGENTS.md               # Developer guidelines
├── CLAUDE.md               # Claude Code entry point
└── README.md               # Project overview
```
