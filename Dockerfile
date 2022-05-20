# This Dockerfile contains Build and Release steps:
# 1. Build image(https://hub.docker.com/_/microsoft-dotnet-core-sdk/)
FROM mcr.microsoft.com/dotnet/sdk:6.0.300-alpine3.15-amd64 AS build
WORKDIR /source

# Cache nuget restore
COPY /src/Endpoint-availability-to-app-insights/*.csproj .
RUN dotnet restore Endpoint-availability-to-app-insights.csproj

# Copy sources and compile
COPY /src/Endpoint-availability-to-app-insights .
RUN dotnet publish Endpoint-availability-to-app-insights.csproj --output /app/ --configuration Release

# 2. Release image
FROM mcr.microsoft.com/dotnet/aspnet:6.0.5-alpine3.15-amd64
WORKDIR /app

# Copy content from Build image
COPY --from=build /app .

ENTRYPOINT ["Endpoint-availability-to-app-insights.exe"]
