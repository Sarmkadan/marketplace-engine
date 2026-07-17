#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MarketplaceEngine.Tests;

/// <summary>
/// Validation helpers for <see cref="PaymentServiceTestsData"/> test data classes.
/// Provides comprehensive validation for test configurations and test data inputs.
/// </summary>
public static class PaymentServiceTestsValidation
{
    /// <summary>
    /// Validates a <see cref="PaymentServiceTestsData"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="PaymentServiceTestsData"/> instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PaymentServiceTestsData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Guid properties are not empty/default
        if (value.ListingIdForNotFoundTest == Guid.Empty)
        {
            problems.Add("ListingIdForNotFoundTest must be a non-empty GUID.");
        }

        if (value.BuyerIdForNotFoundTest == Guid.Empty)
        {
            problems.Add("BuyerIdForNotFoundTest must be a non-empty GUID.");
        }

        if (value.ListingIdForInactiveTest == Guid.Empty)
        {
            problems.Add("ListingIdForInactiveTest must be a non-empty GUID.");
        }

        if (value.BuyerIdForSellerTest == Guid.Empty)
        {
            problems.Add("BuyerIdForSellerTest must be a non-empty GUID.");
        }

        if (value.SellerIdForSellerTest == Guid.Empty)
        {
            problems.Add("SellerIdForSellerTest must be a non-empty GUID.");
        }

        if (value.BuyerIdForValidTest == Guid.Empty)
        {
            problems.Add("BuyerIdForValidTest must be a non-empty GUID.");
        }

        if (value.SellerIdForValidTest == Guid.Empty)
        {
            problems.Add("SellerIdForValidTest must be a non-empty GUID.");
        }

        if (value.ListingIdForValidTest == Guid.Empty)
        {
            problems.Add("ListingIdForValidTest must be a non-empty GUID.");
        }

        if (value.PaymentIdForCancelTest == Guid.Empty)
        {
            problems.Add("PaymentIdForCancelTest must be a non-empty GUID.");
        }

        if (value.BuyerIdForCancelTest == Guid.Empty)
        {
            problems.Add("BuyerIdForCancelTest must be a non-empty GUID.");
        }

        if (value.OtherUserIdForCancelTest == Guid.Empty)
        {
            problems.Add("OtherUserIdForCancelTest must be a non-empty GUID.");
        }

        if (value.PaymentIdForRefundTest == Guid.Empty)
        {
            problems.Add("PaymentIdForRefundTest must be a non-empty GUID.");
        }

        if (value.PaymentIdForCompleteTest == Guid.Empty)
        {
            problems.Add("PaymentIdForCompleteTest must be a non-empty GUID.");
        }

        if (value.SellerIdForCompleteTest == Guid.Empty)
        {
            problems.Add("SellerIdForCompleteTest must be a non-empty GUID.");
        }

        if (value.ListingIdForCompleteTest == Guid.Empty)
        {
            problems.Add("ListingIdForCompleteTest must be a non-empty GUID.");
        }

        // Validate string properties are not null or empty
        if (string.IsNullOrWhiteSpace(value.PaymentMethodForNotFoundTest))
        {
            problems.Add("PaymentMethodForNotFoundTest must not be null or empty.");
        }
        else if (value.PaymentMethodForNotFoundTest.Length > 100)
        {
            problems.Add("PaymentMethodForNotFoundTest exceeds maximum length of 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.RefundReason))
        {
            problems.Add("RefundReason must not be null or empty.");
        }
        else if (value.RefundReason.Length > 500)
        {
            problems.Add("RefundReason exceeds maximum length of 500 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.ValidTransactionId))
        {
            problems.Add("ValidTransactionId must not be null or empty.");
        }
        else if (value.ValidTransactionId.Length > 100)
        {
            problems.Add("ValidTransactionId exceeds maximum length of 100 characters.");
        }

        // Validate complex objects
        if (value.InactiveListing is null)
        {
            problems.Add("InactiveListing must not be null.");
        }
        else
        {
            var listingProblems = ValidateListing(value.InactiveListing);
            problems.AddRange(listingProblems);
        }

        if (value.ListingWithMatchingSeller is null)
        {
            problems.Add("ListingWithMatchingSeller must not be null.");
        }
        else
        {
            var listingProblems = ValidateListing(value.ListingWithMatchingSeller);
            problems.AddRange(listingProblems);
        }

        if (value.ActiveListing is null)
        {
            problems.Add("ActiveListing must not be null.");
        }
        else
        {
            var listingProblems = ValidateListing(value.ActiveListing);
            problems.AddRange(listingProblems);
        }

        if (value.PendingPayment is null)
        {
            problems.Add("PendingPayment must not be null.");
        }
        else
        {
            var paymentProblems = ValidatePayment(value.PendingPayment);
            problems.AddRange(paymentProblems);
        }

        if (value.PendingPaymentForRefundTest is null)
        {
            problems.Add("PendingPaymentForRefundTest must not be null.");
        }
        else
        {
            var paymentProblems = ValidatePayment(value.PendingPaymentForRefundTest);
            problems.AddRange(paymentProblems);
        }

        if (value.ProcessingPayment is null)
        {
            problems.Add("ProcessingPayment must not be null.");
        }
        else
        {
            var paymentProblems = ValidatePayment(value.ProcessingPayment);
            problems.AddRange(paymentProblems);
        }

        if (value.ActiveListingForCompleteTest is null)
        {
            problems.Add("ActiveListingForCompleteTest must not be null.");
        }
        else
        {
            var listingProblems = ValidateListing(value.ActiveListingForCompleteTest);
            problems.AddRange(listingProblems);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="Listing"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="listing">The <see cref="Listing"/> instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    private static IReadOnlyList<string> ValidateListing(Listing? listing)
    {
        var problems = new List<string>();

        if (listing is null)
        {
            problems.Add("Listing must not be null.");
            return problems.AsReadOnly();
        }

        if (listing.Id == Guid.Empty)
        {
            problems.Add("Listing.Id must be a non-empty GUID.");
        }

        if (listing.SellerId == Guid.Empty)
        {
            problems.Add("Listing.SellerId must be a non-empty GUID.");
        }

        if (listing.Status == default)
        {
            problems.Add("Listing.Status must not be default (unset).");
        }

        if (listing.Price is null)
        {
            problems.Add("Listing.Price must not be null.");
        }
        else
        {
            var moneyProblems = ValidateMoney(listing.Price);
            problems.AddRange(moneyProblems);
        }

        if (listing.CreatedAt == default)
        {
            problems.Add("Listing.CreatedAt must not be default DateTime.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="Payment"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="payment">The <see cref="Payment"/> instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    private static IReadOnlyList<string> ValidatePayment(Payment? payment)
    {
        var problems = new List<string>();

        if (payment is null)
        {
            problems.Add("Payment must not be null.");
            return problems.AsReadOnly();
        }

        if (payment.Id == Guid.Empty)
        {
            problems.Add("Payment.Id must be a non-empty GUID.");
        }

        if (payment.ListingId == Guid.Empty)
        {
            problems.Add("Payment.ListingId must be a non-empty GUID.");
        }

        if (payment.BuyerId == Guid.Empty)
        {
            problems.Add("Payment.BuyerId must be a non-empty GUID.");
        }

        if (payment.SellerId == Guid.Empty)
        {
            problems.Add("Payment.SellerId must be a non-empty GUID.");
        }

        if (payment.Amount is null)
        {
            problems.Add("Payment.Amount must not be null.");
        }
        else
        {
            var moneyProblems = ValidateMoney(payment.Amount);
            problems.AddRange(moneyProblems);
        }

        if (payment.PlatformFee is not null)
        {
            var moneyProblems = ValidateMoney(payment.PlatformFee);
            problems.AddRange(moneyProblems);
        }

        if (payment.SellerPayout is not null)
        {
            var moneyProblems = ValidateMoney(payment.SellerPayout);
            problems.AddRange(moneyProblems);
        }

        if (payment.Status == default)
        {
            problems.Add("Payment.Status must not be default.");
        }

        if (payment.CreatedAt == default)
        {
            problems.Add("Payment.CreatedAt must not be default DateTime.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a <see cref="Money"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="money">The <see cref="Money"/> instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    private static IReadOnlyList<string> ValidateMoney(Money? money)
    {
        var problems = new List<string>();

        if (money is null)
        {
            problems.Add("Money must not be null.");
            return problems.AsReadOnly();
        }

        if (money.Amount < 0)
        {
            problems.Add("Money.Amount must not be negative.");
        }

        if (string.IsNullOrWhiteSpace(money.CurrencyCode))
        {
            problems.Add("Money.CurrencyCode must not be null or empty.");
        }
        else if (money.CurrencyCode.Length != 3)
        {
            problems.Add("Money.CurrencyCode must be exactly 3 characters.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="PaymentServiceTestsData"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="PaymentServiceTestsData"/> instance to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PaymentServiceTestsData value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that a <see cref="PaymentServiceTestsData"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The <see cref="PaymentServiceTestsData"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if value contains validation problems.</exception>
    public static void EnsureValid(this PaymentServiceTestsData value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PaymentServiceTestsData validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}