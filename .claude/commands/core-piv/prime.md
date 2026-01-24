---
description: Prime agent with codebase understanding
---

# Prime: Load Project Context

## Objective

Build comprehensive understanding of the codebase by analyzing structure, documentation, and key files.

## Process

### 1. Analyze Project Structure

List all tracked files:
!`git ls-files`

### 2. Read Core Documentation

- Read CLAUDE.md or similar global rules file
- Read README files at project root and major directories
- Read any architecture documentation

### 3. Identify Key Files

Based on the structure, identify and read:
- Main entry points (Program.cs, index.ts, etc.)
- Core configuration files (Directory.Build.props, package.json, tsconfig.json)
- Key Project Folders
  - **Backend API**: `src/Asm.MooBank.Web.Api/` - ASP.NET Core Minimal API
  - **Domain Logic**: `src/Asm.MooBank.Domain/` - DDD entities, aggregates, specifications
  - **Infrastructure**: `src/Asm.MooBank.Infrastructure/` - EF Core repositories, data access
  - **Business Modules**: `src/Asm.MooBank.Modules.*` - CQRS commands, queries, endpoints
  - **Frontend**: `src/Asm.MooBank.Web.App/` - React TypeScript SPA
  - **Database**: `src/Asm.MooBank.Database/` - SQL Server database project

### 4. Understand Current State

Check recent activity:
!`git log -10 --oneline`

Check current branch and status:
!`git status`

## Output Report

Provide a concise summary covering:

### Project Overview
- Purpose and type of application
- Primary technologies and frameworks
- Current version/state

### Architecture
- Overall structure and organization
- Key architectural patterns identified
- Important directories and their purposes

### Tech Stack
- Languages and versions
- Frameworks and major libraries
- Build tools and package managers
- Testing frameworks

### Core Principles
- Code style and conventions observed
- Documentation standards
- Testing approach

### Current State
- Active branch
- Recent changes or development focus
- Any immediate observations or concerns

**Make this summary easy to scan - use bullet points and clear headers.**
## Instructions

1. Read the root CLAUDE.md file
2. Read the root AGENTS.md file
4. Read `.claude/PRD.md` for product requirements overview
5. Identify the key files relevant to the current task using grep/find
6. Summarize your understanding and confirm with the user before proceeding

## Key Project Areas

- **Backend API**: `src/Asm.MooBank.Web.Api/` - ASP.NET Core Minimal API
- **Domain Logic**: `src/Asm.MooBank.Domain/` - DDD entities, aggregates, specifications
- **Infrastructure**: `src/Asm.MooBank.Infrastructure/` - EF Core repositories, data access
- **Business Modules**: `src/Asm.MooBank.Modules.*` - CQRS commands, queries, endpoints
- **Frontend**: `src/Asm.MooBank.Web.App/` - React TypeScript SPA
- **Database**: `src/Asm.MooBank.Database/` - SQL Server database project

### 4. Understand Current State

Check recent activity:
!`git log -10 --oneline`

Check current branch and status:
!`git status`

## Output Report

Provide a concise summary covering:

### Project Overview
- Purpose and type of application
- Primary technologies and frameworks
- Current version/state

### Architecture
- Overall structure and organization
- Key architectural patterns identified
- Important directories and their purposes

### Tech Stack
- Languages and versions
- Frameworks and major libraries
- Build tools and package managers
- Testing frameworks

### Core Principles
- Code style and conventions observed
- Documentation standards
- Testing approach

### Current State
- Active branch
- Recent changes or development focus
- Any immediate observations or concerns

**Make this summary easy to scan - use bullet points and clear headers.**
