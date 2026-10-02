# syntax=docker/dockerfile:1
# Build context: repository root (needs Directory.Build.props and sample_listings.json).

# ---- Build: restore, then publish ---------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Project files first so the restore layer is cached until dependencies change.
COPY Directory.Build.props sample_listings.json ./
COPY src/ListingSearch.Domain/ListingSearch.Domain.csproj src/ListingSearch.Domain/
COPY src/ListingSearch.Application/ListingSearch.Application.csproj src/ListingSearch.Application/
COPY src/ListingSearch.Infrastructure/ListingSearch.Infrastructure.csproj src/ListingSearch.Infrastructure/
COPY src/ListingSearch.Api/ListingSearch.Api.csproj src/ListingSearch.Api/
RUN dotnet restore src/ListingSearch.Api/ListingSearch.Api.csproj
COPY src/ src/
RUN dotnet publish src/ListingSearch.Api/ListingSearch.Api.csproj -c Release -o /app --no-restore

# ---- Runtime: ASP.NET only, no SDK, non-root ---------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# curl is only for the container health check.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=5 \
    CMD curl -fsS http://localhost:8080/health/ready || exit 1
ENTRYPOINT ["dotnet", "ListingSearch.Api.dll"]
