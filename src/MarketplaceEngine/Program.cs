#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Configuration;
using MarketplaceEngine.Middleware;
using MarketplaceEngine.Infrastructure.Background;

var builder = WebApplication.CreateBuilder(args);

// Add Marketplace services and repositories to DI container
builder.Services.AddMarketplaceServices();

// Add full-text search service
builder.Services.AddFullTextSearch();

// Add recommendation engine (collaborative filtering + activity tracking)
builder.Services.AddRecommendationEngine(builder.Configuration);

// Add API documentation (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Controllers for Phase 2
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Marketplace Engine API v1");
        options.RoutePrefix = string.Empty;
    });
}

// Register middleware (order matters!)
// Error handling must be outermost so exceptions thrown by any later
// middleware (logging, rate limiting) or endpoint are converted to the
// standard JSON error envelope instead of escaping as raw 500s.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Map marketplace controllers (Phase 2)
app.MapControllers();

// Map marketplace endpoints (Phase 1)
app.MapMarketplaceEndpoints();

// Map recommendation engine endpoints
app.MapRecommendationEndpoints();

// Map full-text search endpoints
app.MapFullTextSearchEndpoints();

// Subscribe DI-registered domain event handlers to the event bus.
// Without this the handlers are constructed by DI but never receive events.
app.SubscribeMarketplaceEventHandlers();

// Start background job queue
var backgroundQueue = app.Services.GetRequiredService<BackgroundJobQueue>();
backgroundQueue.Start();

// Run the application
try
{
    app.Run();
}
finally
{
    // Ensure background queue is stopped gracefully on shutdown
    await backgroundQueue.StopAsync();
}
