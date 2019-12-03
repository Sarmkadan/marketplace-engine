using System;
using System.Linq;

namespace MarketplaceEngine.Infrastructure.Events
{
    public static class ListingCreatedEventExtensions
    {
        /// <summary>
        /// Gets the listing identifier formatted as a string.
        /// </summary>
        /// <param name="event">The listing created event.</param>
        /// <returns>Formatted listing ID string.</returns>
        public static string GetListingId(this ListingCreatedEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return @event.ListingId.ToString("D");
        }

        /// <summary>
        /// Gets the seller identifier formatted as a string.
        /// </summary>
        /// <param name="event">The listing created event.</param>
        /// <returns>Formatted seller ID string.</returns>
        public static string GetSellerId(this ListingCreatedEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return @event.SellerId.ToString("D");
        }

        /// <summary>
        /// Creates a summary string for the event suitable for logging or display.
        /// </summary>
        /// <param name="event">The listing created event.</param>
        /// <returns>Summary string.</returns>
        public static string ToEventSummary(this ListingCreatedEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return $"[ListingCreated] Listing: {@event.GetListingId()}, Seller: {@event.GetSellerId()}, Title: '{@event.Title}', Category: '{@event.Category}', Occurred: {@event.OccurredAt:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// Determines whether the listing title is valid (not null or whitespace).
        /// </summary>
        /// <param name="event">The listing created event.</param>
        /// <returns>True if the title is valid; otherwise false.</returns>
        public static bool HasValidTitle(this ListingCreatedEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return !string.IsNullOrWhiteSpace(@event.Title);
        }

        /// <summary>
        /// Determines whether the listing category is valid (not null or whitespace).
        /// </summary>
        /// <param name="event">The listing created event.</param>
        /// <returns>True if the category is valid; otherwise false.</returns>
        public static bool HasValidCategory(this ListingCreatedEvent @event)
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            return !string.IsNullOrWhiteSpace(@event.Category);
        }
    }
}