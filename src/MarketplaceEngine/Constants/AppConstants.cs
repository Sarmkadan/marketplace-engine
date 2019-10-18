// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace MarketplaceEngine.Constants;

/// <summary>
/// Contains application-wide constants.
/// </summary>
public static class AppConstants
{
    // Listing constraints
    public const int ListingTitleMinLength = 5;
    public const int ListingTitleMaxLength = 100;
    public const int ListingDescriptionMinLength = 20;
    public const int ListingDescriptionMaxLength = 5000;
    public const int ListingMaxImages = 10;
    public const int ListingMaxTags = 10;

    // User constraints
    public const int UserFullNameMinLength = 2;
    public const int UserFullNameMaxLength = 100;
    public const int UserBioMaxLength = 500;

    // Category constraints
    public const int CategoryNameMinLength = 2;
    public const int CategoryNameMaxLength = 50;

    // Message constraints
    public const int MessageSubjectMinLength = 3;
    public const int MessageSubjectMaxLength = 100;
    public const int MessageBodyMinLength = 5;
    public const int MessageBodyMaxLength = 5000;
    public const int MessageMaxAttachments = 5;

    // Moderation constraints
    public const int ReportReasonMinLength = 5;
    public const int ReportReasonMaxLength = 500;
    public const int ReportMinPriority = 1;
    public const int ReportMaxPriority = 5;

    // Pagination
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int MinPageSize = 1;

    // Cache durations (in seconds)
    public const int CategoryCacheDuration = 3600; // 1 hour
    public const int ListingCacheDuration = 1800; // 30 minutes
    public const int UserCacheDuration = 900; // 15 minutes
    public const int SearchResultsCacheDuration = 300; // 5 minutes

    // Timeouts
    public const int DatabaseTimeoutSeconds = 30;
    public const int ApiRequestTimeoutSeconds = 60;

    // Default values
    public const string DefaultCurrency = "USD";
    public const int DefaultUserRole = 1; // User role
    public const int DefaultListingViewCount = 0;
    public const int DefaultListingInterestCount = 0;

    // Date ranges
    public const int RecentListingDays = 7;
    public const int ArchivedListingRetentionDays = 90;
    public const int MessageRetentionDays = 365;

    // Verification
    public const int EmailVerificationExpiryMinutes = 1440; // 24 hours
    public const int PasswordResetTokenExpiryMinutes = 60;

    // Search
    public const int MaxSearchResults = 1000;
    public const int SearchMinQueryLength = 2;
    public const int SearchMaxQueryLength = 200;

    // Rating
    public const int MinRating = 1;
    public const int MaxRating = 5;
    public const double RatingConfidenceThreshold = 10;

    // API response messages
    public static class Messages
    {
        public const string SuccessfulOperation = "Operation completed successfully";
        public const string ResourceCreated = "Resource created successfully";
        public const string ResourceUpdated = "Resource updated successfully";
        public const string ResourceDeleted = "Resource deleted successfully";
        public const string InvalidInput = "Invalid input provided";
        public const string UnauthorizedAccess = "You do not have permission to perform this action";
        public const string ServerError = "An unexpected error occurred";
    }

    // HTTP status codes
    public static class StatusCodes
    {
        public const int Ok = 200;
        public const int Created = 201;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int InternalServerError = 500;
    }
}
