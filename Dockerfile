# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project files
COPY src/SharedModels/SharedModels.csproj src/SharedModels/
COPY src/ApiGateway/ApiGateway.csproj src/ApiGateway/

# Restore dependencies
RUN dotnet restore src/ApiGateway/ApiGateway.csproj

# Copy source code
COPY src/ src/

# Build
RUN dotnet build src/ApiGateway/ApiGateway.csproj -c Release -o /app/build

# Publish
RUN dotnet publish src/ApiGateway/ApiGateway.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080

# Set default environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
