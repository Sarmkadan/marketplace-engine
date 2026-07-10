using System;

namespace MarketplaceEngine.Exceptions
{
    public static class ResourceNotFoundExceptionExtensions
    {
        public static string GetErrorMessage(this ResourceNotFoundException exception)
        {
            return $"Resource {exception.ResourceType} with ID {exception.ResourceId} not found.";
        }

        public static string GetResourceDetails(this ResourceNotFoundException exception)
        {
            return $"{exception.ResourceType} - {exception.ResourceId}";
        }

        public static bool IsResourceNotFound(this Exception exception)
        {
            return exception is ResourceNotFoundException;
        }
    }
}
