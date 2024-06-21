FROM mcr.microsoft.com/dotnet/sdk:9.0.100-preview.2-bookworm-slim-amd64 AS build
WORKDIR /src
COPY . .
RUN dotnet build "apim-loggin-sim-app.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apim-loggin-sim-app.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0.0-preview.2-bookworm-slim-amd64 AS base
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "apim-loggin-sim-app.dll"]