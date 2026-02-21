using Asm.MooBank.AppHost.Configuration;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var sqlConfig = builder.Configuration.GetSection("SqlServer").Get<SqlServer>() ?? new SqlServer();

IResourceBuilder<IResourceWithConnectionString> mooBankDb;

if (sqlConfig.Enabled)
{
    // Set up SQL Server container

    var sql = builder.AddSqlServer("sql-server", port: 62980)
        .WithLifetime(ContainerLifetime.Persistent);

    if (String.IsNullOrWhiteSpace(sqlConfig.DataBindMount))
    {
        sql = sql.WithDataVolume("MooBankData");
    }
    else
    {
        sql = sql.WithDataBindMount(sqlConfig.DataBindMount);
    }

    mooBankDb = sql.AddDatabase("MooBank");

    //builder.AddSqlProject<Projects.Asm_MooBank_Database>("moobank-database-project")
    //.WithReference(mooBankDb);
}
else
{
    // Use an existing SQL Server

    mooBankDb = builder.AddConnectionString("MooBank");

    //builder.AddSqlProject<Projects.Asm_MooBank_Database>("moobank-database-project")
    //.WithReference(mooBankDb);
}

var api = builder.AddProject<Projects.Asm_MooBank_Web_Api>("moobank-api", "API Only")
    .WithReference(mooBankDb)
    .WithHttpHealthCheck("/healthz");

if (sqlConfig.Enabled)
{
    api.WaitFor(mooBankDb);
}

builder.AddJavaScriptApp("moobank-app", "../Asm.MooBank.Web.App", "start")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(port: 3005, isProxied: false);

builder.Build().Run();
