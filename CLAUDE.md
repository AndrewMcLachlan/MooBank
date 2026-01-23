# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with this codebase.

**You MUST read [AGENTS.md](./AGENTS.md) before writing any code.** It contains mandatory coding conventions and architectural patterns that must be followed.

## Claude Code Commands

See `.claude/commands/` for available slash commands:

| Command | Description |
|---------|-------------|
| `/project:core/prime` | Load project context before implementation |
| `/project:core/plan-feature <desc>` | Create implementation plan for a feature |
| `/project:core/execute <plan-file>` | Execute a plan step-by-step |
| `/project:validation/validate` | Run full validation suite |
| `/project:validation/code-review` | Review changed files |
| `/project:validation/test` | Run test suite |
| `/project:commit` | Create conventional commit |
| `/project:init-project` | First-time project setup and orientation |

## Technology References

Read these files when working on specific areas:

| Reference | Path | When to Read |
|-----------|------|--------------|
| C# | `.claude/reference/csharp.md` | Writing backend code, domain entities, or handlers |
| TypeScript | `.claude/reference/typescript.md` | Working on the React frontend |
| SQL / Database | `.claude/reference/sql-database.md` | Creating tables, modifying schema, or writing migrations |
| REST API | `.claude/reference/rest-api.md` | Adding endpoints, authorization, or OpenAPI documentation |
| Entity Framework | `.claude/reference/entity-framework.md` | Data access, repositories, or specifications |
| CQRS | `.claude/reference/cqrs.md` | Creating commands, queries, or endpoint mappings |

## Additional Resources

- `.claude/PRD.md` - Product requirements and domain overview
- `.agents/plans/` - Feature implementation plans
