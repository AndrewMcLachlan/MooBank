---
paths:
  - "src/Asm.MooBank.Database/**"
---

# SQL Server / Database Design

## Design Principles

### Normalization
- The schema is normalized to reduce redundancy and improve data integrity

### Primary Keys
- The ID of a table is typically a GUID
- Can be an INT for lookup tables or performance-sensitive tables
- The name of the ID column is always `Id`

### Enums as Tables
- C# Enums are represented as lookup tables for flexibility
- Allows adding new values without code changes
- Provides referential integrity

### Seed Data
- Lookup tables are seeded with initial data using post-deployment scripts
- Scripts use MERGE statements to avoid duplication

## Database Project Structure

```
Asm.MooBank.Database/
├── Security/               # Schema definitions
├── {schema-name}/          # The name of a schema, one folder for each
│   ├── Tables/             # Table definitions
│   ├── Functions/          # User-defined functions
│   ├── Stored Procedures/  # Stored procedures
│   └── Views/              # Views
├── Scripts/                # Ad hoc scripts for complex refactoring or filling new tables
└── Script.PostDeployment1/ # Seed data scripts
```

## Migrations

- Database schema is defined in the Database Project
- Changes should be made to SQL files, not generated via EF migrations
- Use database project deployment for schema updates

## Best Practices

- Always define foreign key constraints
- Use appropriate data types (don't use `NVARCHAR(MAX)` when a smaller size suffices)
- Include appropriate indexes for frequently queried columns
- Document complex tables and relationships in comments
