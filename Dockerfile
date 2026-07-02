# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
#
# Multi-stage Docker build for optimized production image
# Uses Alpine-based images for minimal size and security
# =============================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /source

# Install required tools for building
RUN apk add --no-cache \
    git \
    curl \
    bash \
    ca-certificates \
    tzdata

# Set timezone for consistent timestamps
RUN apk add --no-cache tzdata && \
    ln -sf /usr/share/zoneinfo/UTC /etc/localtime && \
    echo "UTC" > /etc/timezone

# Copy project files in two stages for better layer caching
COPY src/MarketplaceEngine/MarketplaceEngine.csproj ./
COPY src/MarketplaceEngine.sln ./
RUN dotnet restore --verbosity minimal

# Copy remaining source files
COPY src/MarketplaceEngine/ ./src/MarketplaceEngine/
COPY tests/ ./tests/

# Build the application in Release mode
RUN dotnet publish -c Release -o /app/publish \
    --no-restore \
    --verbosity minimal

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

# Create non-root user for security
RUN adduser -D -u 1001 appuser
WORKDIR /app

# Install required runtime dependencies
RUN apk add --no-cache \
    curl \
    tzdata \
    libgcc \
    libintl \
    libssl3 \
    libstdc++

# Set timezone
RUN ln -sf /usr/share/zoneinfo/UTC /etc/localtime && \
    echo "UTC" > /etc/timezone

# Copy published application from build stage
COPY --from=build /app/publish .

# Set permissions for non-root user
RUN chown -R appuser:appuser /app && \
    chmod -R 755 /app

# Expose ports
EXPOSE 8080

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV TZ=UTC

# Health check with proper endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/v1/health || exit 1

# Switch to non-root user
USER appuser

# Run the application
ENTRYPOINT ["dotnet", "MarketplaceEngine.dll"]
