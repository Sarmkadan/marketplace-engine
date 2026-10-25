#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using System.Text.Json; // Hotfix: Add missing using directive

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Webhook event that's received from external systems.
/// </summary>
public class WebhookEvent
{
    /// <summary>
    /// Gets or sets the event type name used to route the event to a registered handler (e.g. "payment.completed").
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time at which the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the event payload as key/value pairs.
    /// </summary>
    public Dictionary<string, object?> Data { get; set; } = new();

    /// <summary>
    /// Gets or sets the provider-supplied signature of the event, if any.
    /// </summary>
    public string? Signature { get; set; }
}

/// <summary>
/// Handler interface for webhook events from external services.
/// </summary>
public interface IWebhookEventHandler
{
    /// <summary>
    /// Gets the event type this handler is responsible for.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Handles a received webhook event of the type returned by <see cref="EventType"/>.
    /// </summary>
    /// <param name="webhookEvent">The event to handle.</param>
    /// <returns>A task that completes when the event has been handled.</returns>
    Task HandleAsync(WebhookEvent webhookEvent);
}

/// <summary>
/// Webhook service for receiving and processing webhooks from external providers.
/// Handles signature verification for security.
/// In production, use a library like Svix or AWS SNS for webhook delivery.
/// </summary>
public class WebhookService
{
    private readonly Dictionary<string, IWebhookEventHandler> _handlers = new();
    private readonly ILogger<WebhookService> _logger;
    private readonly string _webhookSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to report webhook processing activity.</param>
    /// <param name="webhookSecret">Shared secret used to compute and verify HMAC-SHA256 signatures.</param>
    public WebhookService(ILogger<WebhookService> logger, string webhookSecret)
    {
        _logger = logger;
        _webhookSecret = webhookSecret;
    }

    /// <summary>
    /// Registers a handler for a specific webhook event type.
    /// </summary>
    public void RegisterHandler(IWebhookEventHandler handler)
    {
        _handlers[handler.EventType] = handler;
        _logger.LogInformation("Webhook handler registered for event type: {EventType}", handler.EventType);
    }

    /// <summary>
    /// Processes an incoming webhook with signature verification.
    /// Signatures prevent webhook spoofing attacks.
    /// </summary>
    public async Task ProcessWebhookAsync(string rawPayload, string? signature)
    {
        try
        {
            // Verify signature to ensure webhook came from trusted source
            if (!VerifySignature(rawPayload, signature))
            {
                _logger.LogWarning("Webhook signature verification failed");
                throw new InvalidOperationException("Invalid webhook signature");
            }

            // Deserialize webhook payload
            var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(rawPayload);
            if (webhookEvent is null)
            {
                _logger.LogWarning("Failed to deserialize webhook payload");
                return;
            }

            _logger.LogInformation("Processing webhook event: {EventType}", webhookEvent.EventType);

            // Find and invoke handler for this event type
            if (_handlers.TryGetValue(webhookEvent.EventType, out var handler))
            {
                await handler.HandleAsync(webhookEvent);
                _logger.LogInformation("Webhook processed successfully: {EventType}", webhookEvent.EventType);
            }
            else
            {
                _logger.LogWarning("No handler registered for webhook event type: {EventType}", webhookEvent.EventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            throw;
        }
    }

    /// <summary>
    /// Verifies webhook signature using HMAC-SHA256.
    /// Prevents unauthorized webhook injection attacks.
    /// </summary>
    private bool VerifySignature(string payload, string? signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Webhook signature is missing");
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            // Use constant-time comparison to prevent timing attacks
            return ConstantTimeEquals(signature, expectedSignature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    /// <summary>
    /// Compares two strings in constant time to prevent timing attacks.
    /// </summary>
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Generates a signature for a payload (useful for testing).
    /// </summary>
    public string GenerateSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}

/// <summary>
/// Example handler for payment provider webhooks (Stripe, PayPal, etc).
/// </summary>
public class PaymentWebhookHandler : IWebhookEventHandler
{
    /// <summary>
    /// Gets the event type handled by this class ("payment.completed").
    /// </summary>
    public string EventType => "payment.completed";

    private readonly ILogger<PaymentWebhookHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentWebhookHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger used to report payment webhook processing.</param>
    public PaymentWebhookHandler(ILogger<PaymentWebhookHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles a "payment.completed" event by extracting the transaction ID and amount from the payload.
    /// </summary>
    /// <param name="webhookEvent">The payment event to handle.</param>
    /// <returns>A task that completes when processing has finished.</returns>
    public async Task HandleAsync(WebhookEvent webhookEvent)
    {
        _logger.LogInformation("Processing payment webhook: {EventType}", webhookEvent.EventType);

        // Extract payment data
        if (webhookEvent.Data.TryGetValue("transactionId", out var transactionId) &&
            webhookEvent.Data.TryGetValue("amount", out var amount))
        {
            _logger.LogInformation("Payment received: transactionId={TransactionId}, amount={Amount}",
                transactionId, amount);

            // In production, this would:
            // 1. Update order status in database
            // 2. Fulfill order (send items, generate shipment, etc)
            // 3. Send confirmation email to buyer
            // 4. Update seller's balance
            // 5. Trigger fulfillment workflow

            await Task.Delay(100); // Simulate processing
        }
    }
}

/// <summary>
/// Example handler for shipping provider webhooks (tracking updates).
/// </summary>
public class ShippingWebhookHandler : IWebhookEventHandler
{
    /// <summary>
    /// Gets the event type handled by this class ("shipment.updated").
    /// </summary>
    public string EventType => "shipment.updated";

    private readonly ILogger<ShippingWebhookHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShippingWebhookHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger used to report shipping webhook processing.</param>
    public ShippingWebhookHandler(ILogger<ShippingWebhookHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles a "shipment.updated" event by extracting the tracking number and status from the payload.
    /// </summary>
    /// <param name="webhookEvent">The shipping event to handle.</param>
    /// <returns>A task that completes when processing has finished.</returns>
    public async Task HandleAsync(WebhookEvent webhookEvent)
    {
        _logger.LogInformation("Processing shipping update: {EventType}", webhookEvent.EventType);

        if (webhookEvent.Data.TryGetValue("trackingNumber", out var tracking) &&
            webhookEvent.Data.TryGetValue("status", out var status))
        {
            _logger.LogInformation("Shipment update: tracking={Tracking}, status={Status}", tracking, status);

            // In production, this would:
            // 1. Update order status
            // 2. Notify buyer of tracking update
            // 3. Trigger delivery confirmation workflow
            // 4. Update reputation scores when delivered

            await Task.Delay(50); // Simulate processing
        }
    }
}
