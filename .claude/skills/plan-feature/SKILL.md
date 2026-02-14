---
name: plan-feature
description: "Create comprehensive feature plan with deep codebase analysis and research"
argument-hint: [feature-description]
context: fork
agent: general-purpose
---

# Plan Feature: $ARGUMENTS

## Mission

Transform a feature request into a **comprehensive implementation plan** through systematic codebase analysis. The plan must contain ALL information needed for one-pass implementation success.

**We do NOT write code in this phase.** Our goal is a context-rich plan that enables the execution agent to succeed on the first attempt.

## Planning Process

### Phase 1: Feature Understanding

- Extract the core problem being solved
- Identify user value and business impact
- Determine feature type: New Capability / Enhancement / Refactor / Bug Fix
- Assess complexity: Low / Medium / High
- Map affected systems and components

Create or refine a user story:

```
As a <type of user>
I want to <action/goal>
So that <benefit/value>
```

**If requirements are unclear, ask the user to clarify before continuing.**

### Phase 2: Codebase Analysis

Use parallel searches and file reads to gather intelligence efficiently:

1. **Read project context** - Read `CLAUDE.md` and `.claude/PRD.md` for architecture and domain understanding
2. **Find similar implementations** - Search for existing patterns that match the feature's domain
3. **Map integration points** - Identify existing files that need updates and where new files should go
4. **Check dependencies** - Catalog external libraries relevant to the feature, check versions
5. **Review test patterns** - Find similar test examples to follow

Key areas to investigate:
- `src/Asm.MooBank.Domain/` - Domain entities, aggregates, specifications
- `src/Asm.MooBank.Modules.*/` - Existing CQRS command/query/endpoint patterns
- `src/Asm.MooBank.Infrastructure/` - Repository implementations
- `src/Asm.MooBank.Web.App/src/` - Frontend components, pages, hooks, services
- `tests/` - Test structure and conventions

**Extract actual code examples** from similar features. Include file paths and line numbers so the execution agent can reference them directly.

### Phase 3: External Research

Only if the feature involves unfamiliar libraries or APIs:
- Research latest documentation with specific section anchors
- Find implementation examples and best practices
- Identify common gotchas and breaking changes

### Phase 4: Generate Plan

Read the plan template at `.claude/skills/plan-feature/template.md` and use it to structure the output. Fill every section with specific, actionable detail from your research.

**Save the plan to:** `.agents/plans/{kebab-case-feature-name}.md`

Create the `.agents/plans/` directory if it doesn't exist.

## Quality Criteria

- [ ] Another developer could implement without additional context
- [ ] Tasks ordered by dependency (execute top-to-bottom)
- [ ] Each task is atomic and independently testable
- [ ] Pattern references include specific file:line numbers
- [ ] Every task has an executable validation command
- [ ] No generic references - all specific and actionable

## Output

After creating the plan, provide:
- Summary of feature and approach
- Full path to created plan file
- Complexity assessment
- Key implementation risks
- Confidence score (/10) for one-pass implementation success
