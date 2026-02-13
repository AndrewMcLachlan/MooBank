---
name: review-implementation
description: "Analyze implementation against plan and generate process improvement recommendations"
argument-hint: [plan-file]
disable-model-invocation: true
context: fork
agent: general-purpose
---

# Review Implementation

Analyze how an implementation compared to its plan, and recommend process improvements.

**This is NOT a code review.** You're looking for bugs in the process, not bugs in the code.

## Inputs

- **Plan file**: `$ARGUMENTS`
- **Current state**: Use `git diff` and `git log` to understand what was actually implemented

## Analysis Process

### Step 1: Understand the Plan

Read the plan file and extract:
- What features were planned?
- What architecture was specified?
- What validation steps were defined?
- What patterns were referenced?

### Step 2: Understand the Implementation

Examine the actual changes:

```bash
git log --oneline -20
git diff HEAD~5 --stat
```

Read modified and new files to understand what was built.

### Step 3: Compare Plan vs Reality

For each planned task:
- Was it implemented as specified?
- Were there deviations? Why?
- Were additional tasks needed that the plan missed?

### Step 4: Classify Divergences

For each divergence, classify as:

**Good Divergence** - Justified:
- Plan assumed something that didn't exist in the codebase
- Better pattern discovered during implementation
- Performance or security improvement needed

**Bad Divergence** - Problematic:
- Ignored explicit constraints in plan
- Created new architecture instead of following existing patterns
- Took shortcuts that introduce tech debt
- Misunderstood requirements

### Step 5: Trace Root Causes

For each problematic divergence:
- Was the plan unclear? Where and why?
- Was context missing? What was needed?
- Was validation missing? What check would have caught this?

### Step 6: Generate Recommendations

Based on patterns across divergences, suggest specific updates to:
- **CLAUDE.md**: Universal patterns or anti-patterns to document
- **Plan skill**: Instructions that need clarification or missing steps
- **Rules files**: Conventions that should be codified
- **New skills**: Manual processes that should be automated

## Output

Save to: `.agents/reviews/{feature-name}-review.md`

### Report Structure

#### Meta Information
- Plan reviewed: [path]
- Date: [current date]
- Files added/modified/deleted

#### Alignment Score: __/10

- 10: Perfect adherence, all divergences justified
- 7-9: Minor justified divergences
- 4-6: Mix of justified and problematic divergences
- 1-3: Major problematic divergences

#### Divergence Analysis

For each divergence:
```yaml
divergence: [what changed]
planned: [what plan specified]
actual: [what was implemented]
classification: good | bad
root_cause: [unclear plan | missing context | etc]
```

#### What Went Well
- [specific things that worked smoothly]

#### Process Improvements

**Update CLAUDE.md:**
- [ ] [specific suggestion]

**Update plan-feature skill:**
- [ ] [specific suggestion]

**Update rules:**
- [ ] [specific suggestion]

#### Key Learnings
- [concrete improvements for next time]

## Important

- Be specific: "plan didn't specify which auth pattern to use" not "plan was unclear"
- Focus on patterns, not one-off issues
- Every finding must have a concrete action item
