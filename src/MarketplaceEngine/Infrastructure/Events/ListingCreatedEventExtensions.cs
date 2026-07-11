using System;

namespace MarketplaceEngine.Infrastructure.Events
{
	/// <summary>
	/// Provides extension methods for <see cref="ListingCreatedEvent"/> to simplify common operations and validations.
	/// </summary>
	public static class ListingCreatedEventExtensions
	{
		/// <summary>
		/// Gets the listing identifier formatted as a string.
		/// </summary>
		/// <param name="event">The listing created event.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
		/// <returns>Formatted listing ID string.</returns>
		public static string GetListingId(this ListingCreatedEvent @event)
		{
			ArgumentNullException.ThrowIfNull(@event);
			return @event.ListingId.ToString("D");
		}

		/// <summary>
		/// Gets the seller identifier formatted as a string.
		/// </summary>
		/// <param name="event">The listing created event.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
		/// <returns>Formatted seller ID string.</returns>
		public static string GetSellerId(this ListingCreatedEvent @event)
		{
			ArgumentNullException.ThrowIfNull(@event);
			return @event.SellerId.ToString("D");
		}

		/// <summary>
		/// Creates a summary string for the event suitable for logging or display.
		/// </summary>
		/// <param name="event">The listing created event.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
		/// <returns>Summary string.</returns>
		public static string ToEventSummary(this ListingCreatedEvent @event)
		{
			ArgumentNullException.ThrowIfNull(@event);

			return $"[ListingCreated] Listing: {@event.GetListingId()}, Seller: {@event.GetSellerId()}, Title: '{@event.Title ?? "(null)"}', Category: '{@event.Category ?? "(null)"}', Occurred: {@event.OccurredAt:yyyy-MM-dd HH:mm:ss}";
		}

		/// <summary>
		/// Determines whether the listing title is valid (not null or whitespace).
		/// </summary>
		/// <param name="event">The listing created event.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
		/// <returns>True if the title is valid; otherwise false.</returns>
		public static bool HasValidTitle(this ListingCreatedEvent @event)
		{
			ArgumentNullException.ThrowIfNull(@event);
			return !string.IsNullOrWhiteSpace(@event.Title);
		}

		/// <summary>
		/// Determines whether the listing category is valid (not null or whitespace).
		/// </summary>
		/// <param name="event">The listing created event.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
		/// <returns>True if the category is valid; otherwise false.</returns>
		public static bool HasValidCategory(this ListingCreatedEvent @event)
		{
			ArgumentNullException.ThrowIfNull(@event);
			return !string.IsNullOrWhiteSpace(@event.Category);
		}
	}
}
