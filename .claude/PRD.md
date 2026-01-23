# Product Requirements Document - MooBank

## Overview

MooBank is a browser-based personal finance management application designed to help individuals track, manage, and analyze their financial activities effectively.

## Key Domain Concepts

- **Instrument** - The base type for all financial products (e.g., bank accounts, credit cards, loans, shares)
- **Transaction Instrument** - A subtype of Instrument that supports transactions (e.g., bank accounts, credit cards)
- **Logical Instrument** - Allows separation between the account and the financial product, allowing you to change institutions without changing accounts
- **Virtual Instrument** - An Instrument that does not represent a real financial product but is used for tracking purposes (e.g., budget categories, savings goals)
- **Institution Account** - Represents a real account at a financial institution, linked to a Logical Instrument
- **Tagging** - A way to categorize and organize transactions for better tracking and reporting
- **Transaction Splits** - Allows a single transaction to be allocated across multiple categories or tags
- **Recurring Transactions** - Scheduled transactions that are automatically created on Virtual Instruments
- **Groups** - The ability to group instruments together and see a total balance for all instruments in the group

## User Flows

1. **Account Setup**: Add institutions, link accounts
2. **Transaction Import**: Upload CSV from financial institutions
3. **Transaction Tagging**: Categorize transactions manually or via rules
4. **Budget Management**: Create budgets, track spending
5. **Reporting**: View spending patterns, trends, forecasts

## Data Models

### Core Entities
- Account/Instrument hierarchy
- Transaction with splits and tags
- Budget and budget periods
- User and Family (multi-tenancy)

### Reference Data
- Tags and tag hierarchies
- Institutions and importers
- Currency and exchange rates

## API Design

RESTful API with:
- `/api/accounts/*` - Account management
- `/api/instruments/*` - Instrument operations
- `/api/transactions/*` - Transaction management
- `/api/tags/*` - Tag management
- `/api/budgets/*` - Budget operations
- `/api/reports/*` - Reporting endpoints

## Security

- **Authentication**: OAuth 2.0 / OpenID Connect via Azure AD
- **Authorization**: Policy-based, with parameterized instrument access
- **Multi-tenancy**: Data isolation via Family/Group membership

## Integrations

### Financial Institution Integrations
These are used to connect to external financial institutions to retrieve account and transaction data:
- Generally involve upload and parsing of CSV files via the `/api/instruments/{id}/import` endpoint
- Each institution has its own project (e.g., `Asm.MooBank.Institution.Ing`)
- Implement the `IImporter` interface
- Store raw transaction data for reprocessing capability
- Located in `Asm.MooBank.Institution.*` projects

**Supported Institutions:**
- ING
- Macquarie
- AustralianSuper

### Reference Data Integrations
These are used to retrieve reference data:
- **Currency exchange rates**: Via `Asm.MooBank.ExchangeRateApi`
- **Inflation data**: Via `Asm.MooBank.Abs` (Australian Bureau of Statistics)
- **Stock prices**: Via `Asm.MooBank.Eodhd`
- Typically use REST APIs
- Run as scheduled background jobs

## Background Jobs

Background jobs are implemented as Azure WebJobs in `Asm.MooBank.Web.Jobs`:

- **Recurring Transactions**: Regularly creates transactions against Virtual Instruments on a set schedule
- **Reference Data Updates**: Regularly updates reference data such as currency exchange rates, inflation data, and stock prices
- Jobs are triggered on a schedule using CRON expressions

## Hosting

The application is hosted on Microsoft Azure using the following services:
- **Azure App Service**: For hosting the web application and API
  - **Deployment Slots**: Used for staging and production environments to facilitate smooth deployments
- **Azure SQL Database**: For the application database
- **Azure WebJobs**: For background job processing
- **Key Vault**: For securely storing application secrets and configuration settings
- **Application Insights**: For monitoring and telemetry

## Build and Deployment

- The build and deployment workflow is defined in `.github/workflows/build.yml`
- Uses GitHub Actions for CI/CD
- Automated deployment to Azure App Service
- Database migrations are applied automatically during deployment

## Testing

- **Unit/Integration Tests**: Located in `tests/` directory
- **BDD Tests**: Using ReqnRoll with `.feature` files for human-readable specifications
- Test projects mirror the structure of the main projects
- Use mocks and fakes for external dependencies
