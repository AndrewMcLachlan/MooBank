FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

COPY ./publish .
ENTRYPOINT ["dotnet", "Asm.MooBank.Web.Api.dll"]
