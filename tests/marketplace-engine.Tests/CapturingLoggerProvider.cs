#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Test logger provider that records every entry written through the loggers it creates,
/// including the structured properties of source-generated <c>LoggerMessage</c> calls.
/// </summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly List<CapturedLogEntry> _entries = new();
    private readonly object _gate = new();

    /// <summary>
    /// Snapshot of all entries captured so far, in the order they were written.
    /// </summary>
    public IReadOnlyList<CapturedLogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToList();
            }
        }
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, this);

    /// <inheritdoc />
    public void Dispose()
    {
    }

    internal void Add(CapturedLogEntry entry)
    {
        lock (_gate)
        {
            _entries.Add(entry);
        }
    }
}

/// <summary>
/// A single log entry captured by <see cref="CapturingLoggerProvider"/>.
/// </summary>
/// <param name="Category">The logger category (usually the full type name).</param>
/// <param name="Level">The severity the entry was written at.</param>
/// <param name="EventId">The event identifier; <see cref="EventId.Name"/> is the generated method name for <c>LoggerMessage</c> calls.</param>
/// <param name="Message">The fully formatted message text.</param>
/// <param name="Properties">Structured properties keyed by template placeholder name.</param>
/// <param name="Exception">The exception attached to the entry, if any.</param>
public sealed record CapturedLogEntry(
    string Category,
    LogLevel Level,
    EventId EventId,
    string Message,
    IReadOnlyDictionary<string, object?> Properties,
    Exception? Exception);

internal sealed class CapturingLogger : ILogger
{
    private const string OriginalFormatKey = "{OriginalFormat}";

    private readonly string _category;
    private readonly CapturingLoggerProvider _provider;

    public CapturingLogger(string category, CapturingLoggerProvider provider)
    {
        _category = category;
        _provider = provider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var properties = new Dictionary<string, object?>();
        if (state is IEnumerable<KeyValuePair<string, object?>> values)
        {
            foreach (var pair in values.Where(p => p.Key != OriginalFormatKey))
                properties[pair.Key] = pair.Value;
        }

        _provider.Add(new CapturedLogEntry(
            _category,
            logLevel,
            eventId,
            formatter(state, exception),
            properties,
            exception));
    }
}
