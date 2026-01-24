# Commit Changes

Create an atomic commit with conventional commit format.

## Instructions

1. Run `git status` to see changes
2. Confirm the branch. Stop and do not commit if the local branch is `main` or is tracking the remote `main` branch
3. Run `git diff --stat` for summary
4. Determine the appropriate commit type:
   - `feature`: New feature
   - `fix`: Bug fix
   - `docs`: Documentation only
   - `style`: Formatting, no code change
   - `refactor`: Code change that neither fixes nor adds
   - `test`: Adding or updating tests
   - `chore`: Maintenance tasks

5. Create commit message: `<type>(<scope>): <description>`

## Scopes

Common scopes for this project:
- `api` - Backend API changes
- `domain` - Domain logic changes
- `infra` - Infrastructure/data access changes
- `web` - Frontend React app changes
- `db` - Database schema changes
- `modules/<name>` - Specific module changes (e.g., `modules/accounts`, `modules/transactions`)

## Rules

- Keep description under 72 characters
- Use imperative mood ("add" not "added")
- Use proper capitalisation and grammar
- Reference issue numbers if applicable: `fix(api): resolve null reference (#123)`
- For breaking changes, add `!` after scope: `feat(api)!: change auth flow`

## Examples

```
feat(modules/budget): Add monthly budget comparison view
fix(domain): Correct balance calculation for splits
refactor(web): Migrate account list to SectionTable component
chore: Update npm dependencies
docs: Add CQRS pattern documentation
```

## Commit Message Format

```bash
git commit -m "$(cat <<'EOF'
<type>(<scope>): <description>

<optional body with more details>
EOF
)"
```
