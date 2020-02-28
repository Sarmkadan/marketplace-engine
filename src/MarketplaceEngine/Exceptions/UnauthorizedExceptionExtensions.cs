using System;

namespace MarketplaceEngine.Exceptions
{
    /// <summary>
    /// Extension methods for <see cref="UnauthorizedException"/>.
    /// </summary>
    public static class UnauthorizedExceptionExtensions
    {
        /// <summary>
        /// Generates a concise log message that includes the user identifier and the attempted action.
        /// </summary>
        /// <param name="exception">The <see cref="UnauthorizedException"/> instance.</param>
        /// <returns>A formatted string suitable for logging.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static string ToLogMessage(this UnauthorizedException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"Unauthorized access: UserId={exception.UserId}, Action=\"{exception.Action}\".";
        }

        /// <summary>
        /// Determines whether the exception pertains to the specified user.
        /// </summary>
        /// <param name="exception">The <see cref="UnauthorizedException"/> instance.</param>
        /// <param name="userId">The user identifier to compare against.</param>
        /// <returns><c>true</c> if the exception's <c>UserId</c> matches <paramref name="userId"/>; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static bool IsForUser(this UnauthorizedException exception, Guid userId)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return exception.UserId == userId;
        }

        /// <summary>
        /// Returns a human‑readable description of the unauthorized action.
        /// </summary>
        /// <param name="exception">The <see cref="UnauthorizedException"/> instance.</param>
        /// <returns>A string describing the attempted action.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static string GetActionDescription(this UnauthorizedException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return $"User attempted unauthorized action: \"{exception.Action}\".";
        }

        /// <summary>
        /// Throws the exception if its <c>UserId</c> does not match the expected identifier.
        /// </summary>
        /// <param name="exception">The <see cref="UnauthorizedException"/> instance.</param>
        /// <param name="expectedUserId">The user identifier that is expected.</param>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/></exception>
        public static void ThrowIfUserMismatch(this UnauthorizedException exception, Guid expectedUserId)
        {
            ArgumentNullException.ThrowIfNull(exception);
            if (exception.UserId != expectedUserId)
            {
                throw exception;
            }
        }
    }
}