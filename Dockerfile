# syntax=docker/dockerfile:1

# 1) Angular SPA. Angular 6 needs Node 10.
FROM node:10 AS spa
WORKDIR /src/HelpDesk/ClientApp
COPY HelpDesk/ClientApp/package.json HelpDesk/ClientApp/package-lock.json ./
RUN npm ci
COPY HelpDesk/ClientApp/ ./
RUN npm run build -- --output-hashing=all

# 2) .NET restore and build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY HelpDesk.sln ./
COPY HelpDesk/HelpDesk.csproj HelpDesk/
COPY HelpDesk.Core/HelpDesk.Core.csproj HelpDesk.Core/
COPY HelpDesk.Infrastructure/HelpDesk.Infrastructure.csproj HelpDesk.Infrastructure/
COPY HelpDesk.Tests/HelpDesk.Tests.csproj HelpDesk.Tests/
RUN dotnet restore HelpDesk.sln
COPY HelpDesk/ HelpDesk/
COPY HelpDesk.Core/ HelpDesk.Core/
COPY HelpDesk.Infrastructure/ HelpDesk.Infrastructure/
COPY HelpDesk.Tests/ HelpDesk.Tests/

# 3) Unit tests: docker build --target tests .
FROM build AS tests
RUN dotnet test HelpDesk.Tests/HelpDesk.Tests.csproj -c Release --no-restore

# 4) Publish the API. The SPA is built in the first stage, so MSBuild skips npm.
FROM build AS publish
RUN dotnet publish HelpDesk/HelpDesk.csproj -c Release --no-restore -o /app/publish -p:SkipSpaBuild=true

# 5) Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
RUN useradd --system --no-create-home --uid 10001 helpdesk
COPY --from=publish /app/publish ./
COPY --from=spa /src/HelpDesk/ClientApp/dist ./ClientApp/dist
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER helpdesk
ENTRYPOINT ["dotnet", "HelpDesk.dll"]
