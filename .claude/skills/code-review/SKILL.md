---
name: code-review
description: "Technical code review for quality and bugs on recently changed files"
disable-model-invocation: true
allowed-tools: Read, Grep, Glob, Bash
---

# Code Review

Perform a technical code review on recently changed files.

## Gather Context

Read CLAUDE.md and key conventions in `.claude/rules/` to understand project standards.

Then examine changes:

```bash
git status
git diff HEAD
git diff --stat HEAD
git ls-files --others --exclude-standard
```

## Review Process

Read each changed and new file **in its entirety** (not just the diff) to understand full context.

For each file, analyze for:

1. **Logic Errors** - Off-by-one, incorrect conditionals, missing error handling, race conditions
2. **Security Issues** - SQL injection, XSS, insecure data handling, exposed secrets
3. **Performance Problems** - N+1 queries, inefficient algorithms, memory leaks
4. **Code Quality** - DRY violations, overly complex functions, poor naming
5. **Pattern Adherence** - CQRS, DDD, module structure, naming conventions, authorization policies

## Verify Issues

- Run specific tests for issues found
- Confirm type errors are legitimate
- Validate security concerns with context

## Output

Save to `.agents/code-reviews/{appropriate-name}.md`

**Stats:**
- Files Modified / Added / Deleted
- Lines added / removed

**For each issue:**

```
severity: critical|high|medium|low
file: path/to/file
line: 42
issue: [one-line description]
detail: [why this is a problem]
suggestion: [how to fix it]
```

If no issues found: "Code review passed. No technical issues detected."

## Important

- Be specific (line numbers, not vague complaints)
- Focus on real bugs, not style preferences
- Suggest fixes, don't just complain
- Flag security issues as CRITICAL
