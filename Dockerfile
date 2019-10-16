# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Build the application in Release mode
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnetcore:10.0
WORKDIR /app

# Copy published application from build stage
COPY --from=build /app/publish .

# Expose ports
EXPOSE 5000 5001

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/api/v1/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "MarketplaceEngine.dll"]
