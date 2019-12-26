using System;
using System.Collections.Generic;

namespace MarketplaceEngine.Exceptions;

public static class MarketplaceExceptionExtensions
{
    public static bool HasValidationErrors(this MarketplaceException exception) 
        => exception.ValidationErrors != null && exception.ValidationErrors.Count > 0;

    public static string GetErrorMessage(this MarketplaceException exception) 
        => $"{exception.Message} {(exception.ErrorCode != null ? $"Error Code: {exception.ErrorCode}" : string.Empty)}";

    public static Dictionary<string, string[]> GetValidationErrors(this MarketplaceException exception) 
        => exception.ValidationErrors ?? new Dictionary<string, string[]>();

    public static string ToErrorString(this MarketplaceException exception) 
    {
        var errorString = exception.GetErrorMessage();
        if (exception.HasValidationErrors())
        {
            errorString += Environment.NewLine + "Validation Errors:";
            foreach (var error in exception.GetValidationErrors())
            {
                errorString += Environment.NewLine + $"  {error.Key}: {string.Join(", ", error.Value)}";
            }
        }
        return errorString;
    }
}
