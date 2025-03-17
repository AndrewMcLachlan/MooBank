# MooBank

![Build](https://github.com/andrewmclachlan/MooBank/actions/workflows/build.yml/badge.svg)

## Getting Started

Please note that this project, including this readme, is a work in progress.

### Prerequisites

* Docker Desktop
* Node JS
* .NET SDK 9.x

### Authentication & Authorisation

Setup an application registration in Azure AD.

Edit the `appsettings.json` file in the `Asm.MooBank.Web.Api` project and add the following configuration:
```json
  "OAuth": {
    "TenantId": "",
    "Domain": "https://login.microsoftonline.com/",
    "Audience": "",
    "ValidateAudience": true
  }
```

Open the `index.tsx` file in the `Asm.MooBank.Web.App`. Find the line below and change the client ID and scopes to match your application registration.

```typescript
<MooApp clientId="045f8afa-70f2-4700-ab75-77ac41b306f7" scopes={["api://moobank.mclachlan.family/api.read"]} name="MooBank" version={import.meta.env.VITE_REACT_APP_VERSION}>
```

This configuration will be simplified in the future.


### Optional Configuration

If you have a Seq server running , you can add the following configuration to the `appsettings.json` file in the `Asm.MooBank.Web.Api` project to log to Seq.

```
  "Seq": {
    "Host": "",
    "ApiKey": ""
  }
```

### Running the application

Run the command:

`dotnet run /src/Asm.MooBank.AppHost/Asm.MooBank.AppHost.csproj`

Aspire will create a new SQL Server container with a database called `MooBank` and publish the database project into it.

A family and account holder will be creatd automatically.