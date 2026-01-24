# Instructions: Expand Claude Code Configuration

You are going to analyze my existing project structure and expand the Claude Code configuration to follow best practices for context engineering.

## Current State

I have:
- `CLAUDE.md` at the project root
- `CLAUDE.md` in the ReactJS subfolder

## Target State

Create a comprehensive `.claude/` configuration structure that maximizes Claude Code's effectiveness.

## Step 1: Analyze Existing Context

First, read and understand:
1. The root `CLAUDE.md` file
2. The ReactJS subfolder `CLAUDE.md` file
3. The overall project structure (run `find . -type f -name "*.md" | head -20` and `ls -la`)

Summarize:
- Tech stack (backend, frontend, database)
- Key patterns and conventions already documented
- Testing approach
- Any gaps in the current documentation

## Step 2: Create Directory Structure

Create the following structure:

```
.claude/
├── commands/
│   ├── core/
│   │   ├── prime.md
│   │   ├── plan-feature.md
│   │   └── execute.md
│   ├── validation/
│   │   ├── validate.md
│   │   ├── code-review.md
│   │   └── test.md
│   ├── commit.md
│   └── init-project.md
├── reference/
│   └── (project-specific docs)
├── settings.local.json
└── PRD.md
```

## Step 3: Create Core Commands

### 3.1 Prime Command (`.claude/commands/core/prime.md`)

```markdown
# Prime Context

Load project context and establish codebase understanding before any implementation work.

## Instructions

1. Read the root CLAUDE.md file
2. Read the relevant subfolder CLAUDE.md (if working in that area)
3. Read .claude/PRD.md for product requirements
4. Identify the key files relevant to the current task using grep/find
5. Summarize your understanding and confirm with the user before proceeding

## Output

Provide a brief summary:
- Current understanding of the task
- Relevant files identified
- Any clarifying questions
```

### 3.2 Plan Feature Command (`.claude/commands/core/plan-feature.md`)

```markdown
# Plan Feature: $ARGUMENTS

Create a comprehensive implementation plan before writing any code.

## Instructions

1. If not already primed, run the prime workflow first
2. Analyze the feature request: $ARGUMENTS
3. Research the codebase for:
   - Similar existing patterns
   - Files that will need modification
   - Dependencies and integrations
4. Create a step-by-step implementation plan

## Output Format

Save the plan to `.agents/plans/<feature-name>.md` with:

### Feature Overview
Brief description of what will be built

### Files to Modify
- List each file with the changes needed

### Files to Create
- List new files with their purpose

### Implementation Steps
1. Numbered steps in order of execution
2. Each step should be atomic and testable

### Validation Criteria
- How to verify the feature works
- Tests to write

### Risks and Considerations
- Edge cases
- Breaking changes
- Performance implications
```

### 3.3 Execute Command (`.claude/commands/core/execute.md`)

```markdown
# Execute Plan: $ARGUMENTS

Execute an implementation plan step-by-step.

## Instructions

1. Read the plan file: $ARGUMENTS (e.g., `.agents/plans/my-feature.md`)
2. Execute each step in order
3. After each step, run relevant tests if available
4. If a step fails, stop and report the issue

## Rules

- Follow the plan exactly unless you identify a critical issue
- If deviating from the plan, explain why before proceeding
- Commit after each logical unit of work (use /project:commit)
- Do not skip validation steps
```

### 3.4 Validate Command (`.claude/commands/validation/validate.md`)

```markdown
# Validate

Run full validation suite on the codebase.

## Instructions

Execute in order, stopping on first failure:

### 1. Linting
- Backend: [ADD YOUR LINT COMMAND]
- Frontend: `cd [react-folder] && npm run lint`

### 2. Type Checking (if applicable)
- `cd [react-folder] && npm run type-check` or `tsc --noEmit`

### 3. Unit Tests
- Backend: [ADD YOUR TEST COMMAND]
- Frontend: `cd [react-folder] && npm test`

### 4. Build Check
- `cd [react-folder] && npm run build`

## Output

Report results as:
- ✅ Passed: [check name]
- ❌ Failed: [check name] - [brief error]
```

### 3.5 Code Review Command (`.claude/commands/validation/code-review.md`)

```markdown
# Code Review

Perform a technical code review on changed files.

## Instructions

1. Run `git diff --name-only HEAD~1` to identify changed files
2. For each changed file, review for:
   - Adherence to project conventions (see CLAUDE.md)
   - Potential bugs or edge cases
   - Performance issues
   - Security concerns
   - Test coverage
3. Provide actionable feedback

## Output Format

### [filename]
- **Issue**: Description
- **Severity**: High/Medium/Low
- **Suggestion**: How to fix

### Summary
- Total issues: X
- Recommendation: Approve / Request Changes
```

### 3.6 Commit Command (`.claude/commands/commit.md`)

```markdown
# Commit Changes

Create an atomic commit with conventional commit format.

## Instructions

1. Run `git status` to see changes
2. Run `git diff --stat` for summary
3. Determine the appropriate commit type:
   - `feat`: New feature
   - `fix`: Bug fix
   - `docs`: Documentation only
   - `style`: Formatting, no code change
   - `refactor`: Code change that neither fixes nor adds
   - `test`: Adding or updating tests
   - `chore`: Maintenance tasks

4. Create commit message: `<type>(<scope>): <description>`

## Rules

- Keep description under 72 characters
- Use imperative mood ("add" not "added")
- Reference issue numbers if applicable
```

## Step 4: Create Settings File

Create `.claude/settings.local.json`:

```json
{
  "permissions": {
    "allow": [
      "Bash(npm run lint)",
      "Bash(npm run test)",
      "Bash(npm run build)",
      "Bash(git status)",
      "Bash(git diff*)",
      "Bash(git log*)",
      "Bash(find*)",
      "Bash(grep*)",
      "Read(*)",
      "Write(.agents/plans/*)"
    ],
    "deny": [
      "Bash(rm -rf*)",
      "Bash(git push*)",
      "Bash(git reset --hard*)"
    ]
  }
}
```

## Step 5: Create PRD Template

Create `.claude/PRD.md` with sections extracted from your existing CLAUDE.md files:

```markdown
# Product Requirements Document

## Overview
[Extract from existing CLAUDE.md or ask user]

## Tech Stack
[List from existing CLAUDE.md]

## Architecture
[Describe or diagram]

## Data Models
[Key entities and relationships]

## API Endpoints (if applicable)
[List key endpoints]

## User Flows
[Key user journeys]
```

## Step 6: Create Plans Directory

```bash
mkdir -p .agents/plans
echo "# Implementation Plans\n\nThis folder contains feature implementation plans generated by Claude Code." > .agents/plans/README.md
```

## Step 7: Update Root CLAUDE.md

Add a section pointing to the new structure:

```markdown
## Claude Code Commands

See `.claude/commands/` for available slash commands:

| Command | Description |
|---------|-------------|
| `/project:core/prime` | Load project context |
| `/project:core/plan-feature <desc>` | Create implementation plan |
| `/project:core/execute <plan-file>` | Execute a plan |
| `/project:validation/validate` | Run full validation |
| `/project:validation/code-review` | Review changed files |
| `/project:commit` | Create conventional commit |
```

## Final Verification

After creating all files:
1. Run `find .claude -type f` to list created files
2. Test one command: try `/project:core/prime`
3. Report what was created and any issues encountered
