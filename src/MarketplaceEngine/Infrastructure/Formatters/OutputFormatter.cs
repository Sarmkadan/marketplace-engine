#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Infrastructure.Formatters;

/// <summary>
/// Enum for supported output formats.
/// </summary>
public enum OutputFormat
{
    Json,
    Csv,
    Xml
}

/// <summary>
/// Interface for output formatters.
/// </summary>
public interface IOutputFormatter
{
    OutputFormat Format { get; }
    string FormatListings(List<ListingDto> listings);
    string FormatListing(ListingDto listing);
}

/// <summary>
/// JSON formatter for listings.
/// </summary>
public class JsonFormatter : IOutputFormatter
{
    public OutputFormat Format => OutputFormat.Json;

    public string FormatListings(List<ListingDto> listings)
    {
        return JsonSerializer.Serialize(listings, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public string FormatListing(ListingDto listing)
    {
        return JsonSerializer.Serialize(listing, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}

/// <summary>
/// CSV formatter for listings.
/// </summary>
public class CsvFormatter : IOutputFormatter
{
    public OutputFormat Format => OutputFormat.Csv;

    public string FormatListings(List<ListingDto> listings)
    {
        var csv = new StringBuilder();

        // Write header
        csv.AppendLine("ID,Title,Description,Price,Seller ID,Seller Name,Category ID,Status,View Count,Created At");

        // Write data rows
        foreach (var listing in listings)
        {
            var row = string.Format(CultureInfo.InvariantCulture,
                "\"{0}\",\"{1}\",\"{2}\",{3},\"{4}\",\"{5}\",\"{6}\",\"{7}\",{8},\"{9}\"",
                EscapeCsv(listing.Id.ToString()),
                EscapeCsv(listing.Title),
                EscapeCsv(listing.Description),
                listing.Price,
                EscapeCsv(listing.SellerId.ToString()),
                EscapeCsv(listing.SellerName),
                EscapeCsv(listing.CategoryId.ToString()),
                EscapeCsv(listing.Status),
                listing.ViewCount,
                listing.CreatedAt.ToString("o"));

            csv.AppendLine(row);
        }

        return csv.ToString();
    }

    public string FormatListing(ListingDto listing)
    {
        return FormatListings(new List<ListingDto> { listing });
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}

/// <summary>
/// XML formatter for listings.
/// </summary>
public class XmlFormatter : IOutputFormatter
{
    public OutputFormat Format => OutputFormat.Xml;

    public string FormatListings(List<ListingDto> listings)
    {
        var root = new XElement("listings");

        foreach (var listing in listings)
        {
            var element = new XElement("listing",
                new XElement("id", listing.Id),
                new XElement("title", listing.Title),
                new XElement("description", listing.Description),
                new XElement("price", listing.Price),
                new XElement("sellerId", listing.SellerId),
                new XElement("sellerName", listing.SellerName),
                new XElement("categoryId", listing.CategoryId),
                new XElement("status", listing.Status),
                new XElement("viewCount", listing.ViewCount),
                new XElement("createdAt", listing.CreatedAt.ToString("o")));

            root.Add(element);
        }

        return root.ToString();
    }

    public string FormatListing(ListingDto listing)
    {
        var element = new XElement("listing",
            new XElement("id", listing.Id),
            new XElement("title", listing.Title),
            new XElement("description", listing.Description),
            new XElement("price", listing.Price),
            new XElement("sellerId", listing.SellerId),
            new XElement("sellerName", listing.SellerName),
            new XElement("categoryId", listing.CategoryId),
            new XElement("status", listing.Status),
            new XElement("viewCount", listing.ViewCount),
            new XElement("createdAt", listing.CreatedAt.ToString("o")));

        return element.ToString();
    }
}

/// <summary>
/// Factory for creating formatters based on requested format.
/// </summary>
public class FormatterFactory
{
    private readonly Dictionary<OutputFormat, IOutputFormatter> _formatters;

    public FormatterFactory()
    {
        _formatters = new Dictionary<OutputFormat, IOutputFormatter>
        {
            { OutputFormat.Json, new JsonFormatter() },
            { OutputFormat.Csv, new CsvFormatter() },
            { OutputFormat.Xml, new XmlFormatter() }
        };
    }

    /// <summary>
    /// Gets a formatter for the specified output format.
    /// </summary>
    public IOutputFormatter GetFormatter(OutputFormat format)
    {
        return _formatters.TryGetValue(format, out var formatter)
            ? formatter
            : _formatters[OutputFormat.Json]; // Default to JSON
    }

    /// <summary>
    /// Gets a formatter by format string (e.g., "json", "csv", "xml").
    /// </summary>
    public IOutputFormatter GetFormatter(string formatString)
    {
        var format = Enum.TryParse<OutputFormat>(formatString, ignoreCase: true, out var result)
            ? result
            : OutputFormat.Json;

        return GetFormatter(format);
    }
}
