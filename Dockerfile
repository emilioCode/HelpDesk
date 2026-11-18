# syntax=docker/dockerfile:1

# 1) Angular SPA. Angular 18 needs Node 18.19, 20.11 or 22 (and the Angular CLI refuses to run on a Node it does not support).
FROM node:20 AS spa
WORKDIR /src/HelpDesk/ClientApp
COPY HelpDesk/ClientApp/package.json HelpDesk/ClientApp/package-lock.json ./
RUN npm ci
COPY HelpDesk/ClientApp/ ./
# The production build: compiled ahead of time (AOT) and optimized. (--prod is deprecated from Angular 12; --configuration=production is the same.)
RUN npm run build -- --configuration=production --output-hashing=all

# 2) .NET restore and build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN useradd --system --no-create-home --uid 10001 helpdesk
COPY --from=publish /app/publish ./
COPY --from=spa /src/HelpDesk/ClientApp/dist ./ClientApp/dist
# The port is the current setting of the runtime image (ASPNETCORE_HTTP_PORTS); ASPNETCORE_URLS would override it with a warning.
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER helpdesk
ENTRYPOINT ["dotnet", "HelpDesk.dll"]
