// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# FAQ - Frequently Asked Questions

Common questions about Marketplace Engine.

## Installation & Setup

### Q: What are the system requirements?

**A:** Minimum requirements:
- **OS**: Windows, macOS, or Linux
- **.NET SDK**: 10.0 or higher
- **RAM**: 2GB minimum (4GB recommended)
- **Disk**: 2GB for project files
- **Optional**: Docker for containerized deployment

### Q: Where can I download .NET 10?

**A:** Visit https://dotnet.microsoft.com/download and download the SDK (not just the runtime).

### Q: Can I use Visual Studio instead of command line?

**A:** Yes! Open `MarketplaceEngine.sln` in Visual Studio 2022 and press F5 to run. VS Code with the C# extension also works.

### Q: How do I change the port from 5000?

**A:** Use environment variable:
```bash
ASPNETCORE_URLS=http://localhost:5002 dotnet run --project src/MarketplaceEngine
```

Or in launchSettings.json:
```json
"applicationUrl": "http://localhost:5002"
```

### Q: Is there a Docker image available?

**A:** Not yet in a public registry. Build locally with `docker build -t marketplace-engine:latest .` or use the provided Dockerfile.

---

## Configuration

### Q: How do I change rate limiting?

**A:** Edit `appsettings.Development.json` or `appsettings.Production.json`:

```json
"MarketplaceConfiguration": {
  "RateLimitRequestsPerMinute": 60
}
```

### Q: Where do I configure the database connection?

**A:** Phase 1 uses in-memory storage. Phase 2 will add database support with configuration in `appsettings.json`:

```json
"Database": {
  "ConnectionString": "Server=...;Database=...;"
}
```

### Q: Can I run multiple instances on one machine?

**A:** Yes, use different ports:
```bash
ASPNETCORE_URLS=http://localhost:5002 dotnet run --project src/MarketplaceEngine &
ASPNETCORE_URLS=http://localhost:5003 dotnet run --project src/MarketplaceEngine &
```

---

## API & Usage

### Q: How do I authenticate requests?

**A:** Phase 1 uses headers:
```
X-User-Id: 1
X-User-Role: Administrator
```

Phase 2 will use JWT bearer tokens.

### Q: What's the rate limit?

**A:** 60 requests per minute by default (configurable).

### Q: Do you support CORS?

**A:** Yes, currently allowing all origins. Configure in `Program.cs`:

```csharp
options.AddPolicy("AllowAll", policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});
```

### Q: Can I use the API with cURL?

**A:** Yes! Example:
```bash
curl -X POST "http://localhost:5000/api/v1/listings" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 1" \
  -d '{"title":"Item","price":{"amount":99.99,"currency":"USD"}}'
```

### Q: How do I get the API documentation?

**A:** Open Swagger UI at `http://localhost:5000` when running in development.

### Q: What's the maximum message length?

**A:** 5000 characters by default (configurable).

### Q: Do messages persist after restart?

**A:** No in Phase 1 (in-memory). Phase 2 adds persistent database.

---

## Performance & Troubleshooting

### Q: Why is the first build slow?

**A:** First build downloads all NuGet packages and compiles the solution. Typically 2-5 minutes. Subsequent builds are much faster (~10 seconds).

### Q: Port 5000 is already in use. What do I do?

**A:** Use a different port:
```bash
ASPNETCORE_URLS=http://localhost:5002 dotnet run --project src/MarketplaceEngine
```

Or find the process using port 5000:
```bash
# On Linux/macOS
lsof -i :5000

# On Windows
netstat -ano | findstr :5000

# Kill the process
kill -9 <PID>  # Linux/macOS
taskkill /PID <PID> /F  # Windows
```

### Q: How do I debug the application?

**A:** 
- In Visual Studio: Press F5 or use Debug menu
- In VS Code: Install C# extension and press F5
- Command line: Use `dotnet run` and add logging statements

### Q: How do I check if the API is running?

**A:** Test the health endpoint:
```bash
curl http://localhost:5000/api/v1/health
```

Expected response:
```json
{"status":"healthy","timestamp":"..."}
```

### Q: Why are some endpoints returning 404?

**A:** Ensure you're using correct endpoint paths. Check Swagger UI for all available endpoints.

### Q: Can I use this in production?

**A:** Phase 1 is suitable for development and testing. For production, migrate to Phase 2 (database) and add authentication, encryption, and monitoring.

---

## Development

### Q: Can I contribute to this project?

**A:** Absolutely! Submit pull requests with:
- Clear description of changes
- Proper code formatting
- Tests if applicable
- Updated documentation

### Q: How do I add a new service?

**A:** 
1. Create service class in `Services/`
2. Create interface (optional but recommended)
3. Register in `Configuration/DependencyInjection.cs`
4. Inject where needed

### Q: How do I add a new API endpoint?

**A:** Add to `Program.cs`:
```csharp
app.MapPost("/api/v1/myendpoint", async (HttpContext context, MyService service) =>
{
    // Implementation
    return Results.Ok();
});
```

### Q: How do I add domain validation?

**A:** Implement validation in domain model constructors:
```csharp
public class Listing
{
    public Listing(string title, string description, Money price)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException("Title is required");
        if (price.Amount <= 0)
            throw new ValidationException("Price must be positive");
        
        Title = title;
        Description = description;
        Price = price;
    }
}
```

### Q: Where should I put utilities?

**A:** Add to `Utilities/` folder with descriptive names:
- `StringUtility.cs` - String operations
- `DateTimeUtility.cs` - Date/time helpers
- `ValidationUtility.cs` - Validation helpers

---

## Deployment

### Q: Can I deploy to Docker?

**A:** Yes! Use provided `Dockerfile`:
```bash
docker build -t marketplace-engine:latest .
docker run -p 5000:5000 marketplace-engine:latest
```

### Q: How do I deploy to Azure?

**A:** See [Deployment Guide](deployment.md) for step-by-step instructions.

### Q: Can I use Kubernetes?

**A:** Yes, manifest in [Deployment Guide](deployment.md).

### Q: What about SSL/HTTPS in production?

**A:** Configure in `appsettings.Production.json`:
```json
"Security": {
  "RequireHttps": true
}
```

Or with environment variable:
```
ASPNETCORE_URLS=https://+:5001;http://+:5000
```

### Q: How do I set up automatic backups?

**A:** Use database-specific tools:
- **SQL Server**: SQL Server Management Studio
- **PostgreSQL**: `pg_dump`
- **MongoDB**: `mongodump`

See [Deployment Guide](deployment.md) for examples.

---

## Features & Roadmap

### Q: When will authentication be added?

**A:** Phase 2 will include JWT authentication and OAuth 2.0.

### Q: Will you support database integration?

**A:** Yes, Phase 2 adds SQL Server/PostgreSQL support.

### Q: What about real-time features?

**A:** Phase 4 will include WebSocket support.

### Q: Is Elasticsearch search planned?

**A:** Yes, Phase 3 will integrate Elasticsearch for advanced search.

### Q: Will you support payments?

**A:** Phase 5 will integrate Stripe for payments.

### Q: What about notifications?

**A:** Phase 7 will add email/SMS notifications.

---

## Technical Questions

### Q: What's the difference between entities and value objects?

**A:** 
- **Entities** have identity (Listing, User, Message)
- **Value Objects** don't (Money, Location, Rating)

### Q: Why use dependency injection?

**A:** Enables testing, loose coupling, and flexibility in swapping implementations.

### Q: What's an async method?

**A:** A method that can pause execution (e.g., waiting for I/O) and resume later, allowing other operations meanwhile. All I/O operations (database, API calls) should be async.

### Q: How do you handle errors?

**A:** Custom exception types map to HTTP status codes:
- `ValidationException` → 400 Bad Request
- `ResourceNotFoundException` → 404 Not Found
- `UnauthorizedException` → 401 Unauthorized
- `DuplicateResourceException` → 409 Conflict

### Q: What's the difference between synchronous and asynchronous?

**A:** 
- **Synchronous**: Code executes line-by-line, blocking on I/O
- **Asynchronous**: Code can continue while waiting for I/O

```csharp
// Synchronous (blocks)
var user = userRepository.GetUser(1);

// Asynchronous (doesn't block)
var user = await userRepository.GetUserAsync(1);
```

---

## Support & Community

### Q: Where can I report bugs?

**A:** Open an issue on GitHub with:
- Description of the bug
- Steps to reproduce
- Expected vs actual behavior
- Your environment (OS, .NET version, etc.)

### Q: How do I request a feature?

**A:** Open a GitHub issue labeled "feature-request" describing:
- What you want to do
- Why it's important
- How you'd use it

### Q: Is there a community forum?

**A:** Currently using GitHub Issues. Consider starting discussions in the repository.

### Q: How often is the project updated?

**A:** Active development. Check GitHub for latest updates.

### Q: Who maintains this project?

**A:** **Vladyslav Zaiets** - CTO & Software Architect. See [README](../README.md) for contact info.

---

## Still Have Questions?

1. **Check the documentation**: [Getting Started](getting-started.md), [Architecture](architecture.md), [API Reference](api-reference.md)
2. **Search GitHub Issues**: Someone may have asked before
3. **Review examples**: Check `examples/` folder for code samples
4. **Create an issue**: Detailed questions can be asked on GitHub

---

**Happy coding! 🚀**
