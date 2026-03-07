FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080

COPY ./publish .
ENTRYPOINT ["dotnet", "Asm.MooBank.Web.Api.dll"]
