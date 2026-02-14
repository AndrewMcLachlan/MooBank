---
name: create-prd
description: "Generate a Product Requirements Document from conversation context"
argument-hint: [output-filename]
---

# Create PRD

Generate a comprehensive Product Requirements Document based on the current conversation context.

## Output File

Write the PRD to: `$ARGUMENTS` (default: `PRD.md`)

## Sections

### Required

1. **Executive Summary** - Product overview, core value proposition, MVP goal
2. **Mission** - Mission statement, core principles (3-5)
3. **Target Users** - Personas, technical comfort, pain points
4. **MVP Scope** - In scope / out of scope, grouped by category
5. **User Stories** - 5-8 primary stories in "As a / I want / So that" format with examples
6. **Core Architecture** - High-level architecture, directory structure, key patterns
7. **Features** - Detailed feature specifications
8. **Technology Stack** - Backend/frontend/infrastructure with versions
9. **Security & Configuration** - Auth, authorization, config management
10. **API Specification** - Endpoints, auth, example payloads (if applicable)
11. **Success Criteria** - Functional requirements, quality indicators, UX goals
12. **Implementation Phases** - 3-4 phases with goals, deliverables, validation
13. **Future Considerations** - Post-MVP enhancements
14. **Risks & Mitigations** - 3-5 key risks with strategies
15. **Appendix** - Related docs, dependencies, repo structure

### Style

- Professional, clear, action-oriented tone
- Use markdown formatting extensively
- Use checkboxes for scope items
- Prefer concrete examples over abstract descriptions

## Process

1. Review entire conversation history
2. Extract explicit and implicit requirements
3. Synthesize into PRD sections
4. Verify completeness: all sections present, user stories have benefits, scope is realistic

## After Creating

1. Confirm file path
2. Summarize contents
3. Highlight assumptions made
4. Suggest next steps
