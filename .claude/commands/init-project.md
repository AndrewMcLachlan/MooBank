# Initialize Project

Get the MooBank project up and running after cloning.

## Instructions

Execute each step in order. Stop and report issues if any step fails.

### Step 1: Check Prerequisites

Verify the following are installed:
```bash
dotnet --version   # Requires .NET 9.0 or later
node --version     # Requires Node.js 18+ (LTS recommended)
npm --version      # Comes with Node.js
```

### Step 2: Restore Backend Dependencies

```bash
dotnet restore Asm.MooBank.slnx
```

**If authentication fails** (401 Unauthorized for GitHub Packages):

The ASM library packages are hosted on GitHub Packages. Instruct the user:

1. Create a GitHub Personal Access Token (PAT) with `read:packages` scope at:
   https://github.com/settings/tokens

2. Add a NuGet source with authentication:
   ```bash
   dotnet nuget add source "https://nuget.pkg.github.com/AndrewMcLachlan/index.json" \
     --name "GitHub-AndrewMcLachlan" \
     --username YOUR_GITHUB_USERNAME \
     --password YOUR_GITHUB_PAT \
     --store-password-in-clear-text
   ```

3. Retry the restore:
   ```bash
   dotnet restore Asm.MooBank.slnx
   ```

### Step 3: Build Backend

```bash
dotnet build Asm.MooBank.slnx --no-restore
```

Confirm build succeeds with no errors.

### Step 4: Install Frontend Dependencies

```bash
cd src/Asm.MooBank.Web.App && npm install
```

**If authentication fails** (401/403 for @andrewmclachlan packages):

The MooApp packages are hosted on GitHub Packages. Instruct the user:

1. Create a GitHub Personal Access Token (PAT) with `read:packages` scope at:
   https://github.com/settings/tokens

2. Create or update `~/.npmrc` (user home directory) with:
   ```
   //npm.pkg.github.com/:_authToken=YOUR_GITHUB_PAT
   ```

3. Retry the install:
   ```bash
   npm install
   ```

### Step 5: Build Frontend

```bash
cd src/Asm.MooBank.Web.App && npm run build
```

Confirm build succeeds with no errors.

### Step 6: Run the Application

Start the backend API:
```bash
dotnet run --project src/Asm.MooBank.Web.Api/Asm.MooBank.Web.Api.csproj
```

In a separate terminal, start the frontend dev server:
```bash
cd src/Asm.MooBank.Web.App && npm start
```

### Step 7: Confirm Startup

Verify both services are running:

**Backend API**: Look for output indicating the server is listening, typically:
```
Now listening on: https://localhost:7005
```

**Frontend**: Look for Vite dev server output:
```
VITE vX.X.X ready
➜ Local: http://localhost:3005/
```

Report the URLs where both services are accessible.

## Troubleshooting

### Database Connection Issues
The API requires a connection to Azure SQL Database. If connection fails:
- Check `appsettings.Development.json` for connection string
- Ensure Azure credentials are configured or local database is available

### Port Conflicts
If ports are in use, the services will fail to start. Check for:
- Port 7005 (API default)
- Port 3005 (Vite default)

## Output

Report:
- Backend build status (success/failure)
- Frontend build status (success/failure)
- URLs where services are running
- Any authentication or connection issues encountered
