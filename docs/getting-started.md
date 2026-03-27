// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with Marketplace Engine

This guide walks you through setting up and running Marketplace Engine for the first time.

## Prerequisites

Before you begin, ensure you have:

- **Operating System**: Windows, macOS, or Linux
- **.NET 10 SDK**: Download from https://dotnet.microsoft.com/download
- **Git**: Version control system (https://git-scm.com)
- **IDE** (Optional but recommended):
  - Visual Studio 2022 (Windows)
  - Visual Studio Code (All platforms)
  - JetBrains Rider (All platforms)
- **4GB RAM** minimum (8GB recommended)
- **2GB disk space** for the project

## Step 1: Verify Prerequisites

### Check .NET Installation

```bash
dotnet --version
```

Expected output: `8.0.0` or higher (we're using .NET 10)

### Check Git Installation

```bash
git --version
```

Expected output: `git version 2.x.x`

## Step 2: Clone the Repository

```bash
# Navigate to your projects directory
cd ~/projects

# Clone the repository
git clone https://github.com/Sarmkadan/marketplace-engine.git

# Navigate into project
cd marketplace-engine

# Verify structure
ls -la
```

You should see:
```
.git/
.gitignore
LICENSE
README.md
MarketplaceEngine.sln
src/
docs/
examples/
```

## Step 3: Restore Dependencies

```bash
# From project root directory
dotnet restore

# Or with verbose output for debugging
dotnet restore --verbosity diagnostic
```

This downloads all NuGet packages required by the project.

## Step 4: Build the Solution

```bash
# Standard debug build
dotnet build

# Verbose build (good for debugging)
dotnet build --verbosity normal

# Release build (optimized)
dotnet build -c Release
```

Expected output ends with:
```
Build succeeded.
```

## Step 5: Run the Application

### From Project Root

```bash
dotnet run --project src/MarketplaceEngine
```

### Or Navigate and Run

```bash
cd src/MarketplaceEngine
dotnet run
```

You should see output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

## Step 6: Access the Application

### Via Web Browser

1. Open your browser
2. Navigate to: `http://localhost:5000`
3. You'll see the Swagger UI homepage

### Test Health Endpoint

```bash
curl http://localhost:5000/api/v1/health
```

Expected response:
```json
{
  "status": "healthy",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

## Step 7: Explore the API

### Swagger UI

The interactive API documentation is available at `http://localhost:5000`:

1. **Expand** any endpoint category (Listings, Users, etc.)
2. **Click Try it out** to test endpoints
3. **View** request/response format
4. **Test** with sample data

### Key Endpoints to Try

#### Get All Listings
```bash
curl -X GET "http://localhost:5000/api/v1/listings" \
  -H "accept: application/json"
```

#### Search Listings
```bash
curl -X GET "http://localhost:5000/api/v1/listings/search?q=test" \
  -H "accept: application/json"
```

#### Get Categories
```bash
curl -X GET "http://localhost:5000/api/v1/categories" \
  -H "accept: application/json"
```

## Step 8: Development Workflow

### Opening in Visual Studio Code

```bash
# From project root
code .
```

### Opening in Visual Studio 2022

1. Double-click `MarketplaceEngine.sln`
2. Wait for solution to load
3. Press `F5` to start debugging

### Using Command Line

```bash
# Start with debugging
dotnet run --project src/MarketplaceEngine

# Start optimized (release)
dotnet build -c Release
dotnet bin/Release/net10.0/MarketplaceEngine.dll

# Run tests (when available in Phase 2)
dotnet test
```

## Step 9: Making Your First API Call

### Create a Listing (via cURL)

```bash
curl -X POST "http://localhost:5000/api/v1/listings" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{
    "title": "My First Item",
    "description": "This is my first marketplace listing",
    "price": {
      "amount": 99.99,
      "currency": "USD"
    },
    "category": "Electronics",
    "tags": ["test", "first"],
    "location": {
      "city": "New York",
      "country": "USA"
    }
  }'
```

### Check Response

You should get a 201 Created response with the new listing ID:

```json
{
  "success": true,
  "message": "Listing created successfully",
  "data": {
    "id": 1,
    "title": "My First Item",
    "sellerId": 1,
    "status": "Active"
  }
}
```

## Step 10: Configuration

### Development Configuration

Edit `src/MarketplaceEngine/appsettings.Development.json`:

```json
{
  "MarketplaceConfiguration": {
    "MaxListingsPerUser": 100,
    "RateLimitRequestsPerMinute": 1000
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "MarketplaceEngine": "Debug"
    }
  }
}
```

### Environment Variables

Set environment variables for different environments:

```bash
# Development (default)
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5000

# Then run
dotnet run --project src/MarketplaceEngine
```

## Troubleshooting

### Problem: "No such file or directory"

```
error: pathspec 'marketplace-engine' did not match any files
```

**Solution**: Ensure you're cloning from the correct URL:
```bash
git clone https://github.com/Sarmkadan/marketplace-engine.git
```

### Problem: "dotnet: command not found"

**Solution**: .NET SDK not installed or not in PATH
```bash
# Install .NET 10 SDK from https://dotnet.microsoft.com/download
# Then verify installation
dotnet --version
```

### Problem: "No NuGet sources available"

**Solution**: Restore from command line explicitly:
```bash
dotnet restore --sources https://api.nuget.org/v3/index.json
```

### Problem: Port 5000 in use

```
error: listen EADDRINUSE: address already in use :::5000
```

**Solution**: Use different port:
```bash
dotnet run --project src/MarketplaceEngine -- --urls "http://localhost:5002"
```

### Problem: HTTPS certificate error

```
error: unable to configure HTTPS endpoint. No server certificate was specified
```

**Solution**: For development, disable HTTPS:
```bash
ASPNETCORE_URLS=http://localhost:5000 dotnet run --project src/MarketplaceEngine
```

### Problem: Slow first build

**Solution**: This is normal. First build:
- Downloads all NuGet packages
- Compiles all projects
- Takes 2-5 minutes on first run
- Subsequent builds are much faster (~10 seconds)

## Next Steps

1. **Explore Examples**: Check `examples/` directory for complete code samples
2. **Read Architecture**: Review `docs/architecture.md` for system design
3. **API Reference**: See `docs/api-reference.md` for complete endpoint documentation
4. **Configuration**: Customize `appsettings.json` for your needs
5. **Contribute**: Make improvements and submit pull requests

## Support

If you encounter issues:

1. Check **Troubleshooting** section above
2. Review existing GitHub Issues
3. Check project's `docs/faq.md`
4. Create a new GitHub Issue with:
   - Your OS and .NET version
   - Complete error message
   - Steps to reproduce
   - Expected vs actual behavior

## Quick Reference Commands

```bash
# Clone and setup
git clone https://github.com/Sarmkadan/marketplace-engine.git
cd marketplace-engine
dotnet restore
dotnet build

# Run application
dotnet run --project src/MarketplaceEngine

# Run with specific port
ASPNETCORE_URLS=http://localhost:5002 dotnet run --project src/MarketplaceEngine

# Build release version
dotnet build -c Release

# Run tests (when available)
dotnet test

# Clean build artifacts
dotnet clean

# Update packages
dotnet add package PackageName --version 1.0.0
```

---

**Happy coding! 🚀**
