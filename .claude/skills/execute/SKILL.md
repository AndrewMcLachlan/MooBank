---
name: execute
description: "Execute an implementation plan step-by-step"
argument-hint: [path-to-plan]
---

# Execute Plan

## Plan File

Read the plan: `$ARGUMENTS`

## Process

### 1. Understand the Plan

- Read the ENTIRE plan carefully
- Understand all tasks and their dependencies
- Note the validation commands
- Read all files listed under "Mandatory Reading"

### 2. Execute Tasks in Order

For EACH task in "Step-by-Step Tasks":

**a. Prepare**
- Read existing files before modifying them
- Understand the context and integration points

**b. Implement**
- Follow the specifications exactly
- Maintain consistency with existing code patterns
- Use the pattern references provided in the plan

**c. Verify**
- After each file change, check syntax and imports
- Run the task's VALIDATE command if provided
- Fix issues before moving to the next task

### 3. Run Validation Commands

Execute ALL validation commands from the plan in order:

If any command fails:
- Fix the issue
- Re-run the command
- Continue only when it passes

### 4. Final Check

- [ ] All tasks from plan completed
- [ ] All tests passing
- [ ] All validation commands pass
- [ ] Code follows project conventions
- [ ] No compiler warnings

## Output

Provide a summary:

### Completed Tasks
- List of all tasks completed
- Files created (with paths)
- Files modified (with paths)

### Validation Results
- Output from each validation command

### Issues Encountered
- Any deviations from the plan and why
- Any issues discovered during implementation

### Ready for Review
- Confirm all changes are complete and validated
- Suggest running `/code-review` and `/commit`
