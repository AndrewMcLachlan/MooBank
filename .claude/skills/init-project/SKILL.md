---
name: init-project
description: "First-time project setup after cloning"
disable-model-invocation: true
---

# Initialize Project

Execute each step in order. Stop and report issues if any step fails.

## Step 1: Check Prerequisites

```bash
dotnet --version   # Requires .NET 9.0 or later
node --version     # Requires Node.js 18+ (LTS recommended)
npm --version
```

## Step 2: Restore & Build Backend

```bash
dotnet restore Asm.MooBank.slnx
dotnet build Asm.MooBank.slnx --no-restore
```

**If restore fails** (401 Unauthorized for GitHub Packages):

ASM library packages are hosted on GitHub Packages. Instruct the user to:
1. Create a GitHub PAT with `read:packages` scope at https://github.com/settings/tokens
2. Add the NuGet source:
   ```bash
   dotnet nuget add source "https://nuget.pkg.github.com/AndrewMcLachlan/index.json" \
     --name "GitHub-AndrewMcLachlan" \
     --username YOUR_GITHUB_USERNAME \
     --password YOUR_GITHUB_PAT \
     --store-password-in-clear-text
   ```
3. Retry: `dotnet restore Asm.MooBank.slnx`

## Step 3: Install & Build Frontend

```bash
cd src/Asm.MooBank.Web.App && npm install
cd src/Asm.MooBank.Web.App && npm run build
```

**If npm install fails** (401/403 for @andrewmclachlan packages):

MooApp packages are hosted on GitHub Packages. Instruct the user to:
1. Create a GitHub PAT with `read:packages` scope
2. Add to `~/.npmrc`: `//npm.pkg.github.com/:_authToken=YOUR_GITHUB_PAT`
3. Retry: `npm install`

## Step 4: Run the Application

```bash
# Backend
dotnet run --project src/Asm.MooBank.Web.Api/Asm.MooBank.Web.Api.csproj

# Frontend (separate terminal)
cd src/Asm.MooBank.Web.App && npm start
```

## Step 5: Confirm

- Backend: `https://localhost:7005`
- Frontend: `http://localhost:3005/`

## Troubleshooting

- **Database**: Check `appsettings.Development.json` for connection string
- **Port conflicts**: API uses 7005, Vite uses 3005

## Report

- Backend build: PASS/FAIL
- Frontend build: PASS/FAIL
- Service URLs
- Any issues encountered
