// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;

namespace MarketplaceEngine.Infrastructure.Integration;

/// <summary>
/// Webhook event that's received from external systems.
/// </summary>
public class WebhookEvent
{
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public string? Signature { get; set; }
}

/// <summary>
/// Handler interface for webhook events from external services.
/// </summary>
public interface IWebhookEventHandler
{
    string EventType { get; }
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
            if (webhookEvent == null)
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
    public string EventType => "payment.completed";

    private readonly ILogger<PaymentWebhookHandler> _logger;

    public PaymentWebhookHandler(ILogger<PaymentWebhookHandler> logger)
    {
        _logger = logger;
    }

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
    public string EventType => "shipment.updated";

    private readonly ILogger<ShippingWebhookHandler> _logger;

    public ShippingWebhookHandler(ILogger<ShippingWebhookHandler> logger)
    {
        _logger = logger;
    }

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
