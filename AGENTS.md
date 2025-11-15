# Overview

MooBank is a browser-based application designed to enable an individual to manage their personal finances effectively.

# Key concepts

- Instrument - The base type for all financial products (e.g., bank accounts, credit cards, loans, shares).
- Transaction Instrument - A subtype of Instrument that supports transactions (e.g., bank accounts, credit cards).
- Logical Instrument - Allows separation between the account and the financial product, allowing you to change institutions without changing accounts.
- Virtual Instrument - An Instrument that does not represent a real financial product but is used for tracking purposes (e.g., budget categories, savings goals).
- Tagging - A way to categorize and organize transactions for better tracking and reporting.

# Architecure

## Technology Stack

The technology stack for MooBank includes:
- Frontend: React.js for building a responsive and interactive user interface.
- Backend: .NET / ASP.NET Core. MooBank will generally use the latest general-availability version of .NET.
- Database: Azure SQL

## Software Design Patterns

The software design patterns employed in MooBank include:

- Minimal API
- Command Query Responsibility Segregation (CQRS)
- Domain-Driven Design (DDD)
- Unit of Work and Repository Patterns

## Code layout

The codebase is organized into the following main projects:
- Asm.MooBank.Web.Api: The ASP.NET Core Web API project. This is intentionally kept minimal, with most of the business logic contained in other projects.
- Asm.MooBank.Domain: Contains the core domain logic, including entities, value objects, aggregates, specifications etc. It specifically avoids dependencies on infrastructure concerns.
- Asm.MooBank.Infrastructure: Contains implementations for data access, including database context, repsository implementations, and migrations.
- Asm.MooBank.Modules.*: A set of projects that encapsulate specific business capabilities or modules. Each module contains its own commands, queries and handlers.

## Libraries and Frameworks

### Backend

- Entity Framework Core: An Object-Relational Mapper (ORM) for database access.
- ASM Library. A set of custom NuGet packages developed to hide boilerplate and simplify common tasks. These include, but are not limited to:
    - Asm.AspNetCore - For application boostrapping, including OpenTelemetry and health checks.
    - Asm.Cqrs.AspNetCore - Extensions that allow endpoints to be mapped to CQRS commands and queries.
    - Asm.Domain / Asm.Domain.Infrastructure - Base classes and interfaces for implementing DDD patterns.

  The code for the ASM Library is available at: https://github.com/AndrewMcLachlan/ASM

- ReqnRoll - A behviour-driven development (BDD) framework for writing human-readable unit tests.
- Database Project - The database design is maintained as a first-class component of the application, using the newer SDK-style project format

### Frontend

- React.js: A JavaScript library for building user interfaces.
- React Router: A library for routing in React applications.
- MSAL (Microsoft Authentication Library): A library for handling authentication and authorization in React applications.
- MooApp - A set of custom packages of React components and hooks to simplify application development. The code for MooApp is available at: https://github.com/AndrewMcLachlan/MooApp

## Security

- Authentication: OAuth 2.0 and OpenID Connect for secure user authentication.
- Authorization: Policy-based authorization to control access to resources based on user roles and permissions.

## Hosting

The application is hosted on Microsoft Azure using the following services:
- Azure App Service: For hosting the web application.
    - Deployment Slots: Used for staging and production environments to facilitate smooth deployments.
- Key Vault: For securely storing application secrets and configuration settings.

## Build and Deployment

- The build and deployment workflow can be found at .github/workflows/build.yml

## Integrations

There are two types of integrations.

1. Financial Institution Integrations - These are used to connect to external financial institutions to retrieve account and transaction data. Generally they involved upload and parsing of CSV files
2. Refgerence Data Integrations - These are used to retrieve reference data such as currency exchange rates, inflation data, and stock prices. These integrations are typically done via REST APIs.

## Background Jobs

Background jobs are run as Azure WebJobs.

- Recurring transactions - The ability to regularly create transactions against a Virtual Instrument on a set schedule.
- Reference data updates - Regularly updating reference data such as currency exchange rates, inflation data, and stock prices.

# Instructions

- Follow the patterns, libraries, frameworks and conventions outlined in this document when contributing to the MooBank codebase.
- Respect the rules defined in .editorconfig and use consistent coding styles.
- Respect the folder and project structure when adding new features or making changes.
