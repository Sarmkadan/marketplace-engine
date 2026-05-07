#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Xunit;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Serializes all test classes that access the shared in-memory
/// <c>MarketplaceDbContext</c> singleton so they do not run in parallel.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection { }
