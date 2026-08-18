using FluentAssertions;
using MarketplaceEngine.DTOs;
using MarketplaceEngine.Exceptions;
using Xunit;

namespace MarketplaceEngine.Tests;

public class ApiResponseExtensionsTests
{
    [Fact]
    public void EnsureSuccess_WhenSuccess_ReturnsResponse()
    {
        // Arrange
        var response = ApiResponse<string>.SuccessResponse("data", "message", "request-1");

        // Act
        var result = response.EnsureSuccess();

        // Assert
        result.Should().BeSameAs(response);
    }

    [Fact]
    public void EnsureSuccess_WhenError_ThrowsMarketplaceException()
    {
        // Arrange
        var response = ApiResponse<string>.ErrorResponse("ERR_CODE", "Error message", "request-1");

        // Act
        Action act = () => response.EnsureSuccess();

        // Assert
        act.Should().Throw<MarketplaceException>()
           .WithMessage("Error message")
           .Which.ErrorCode.Should().Be("ERR_CODE");
    }

    [Fact]
    public void Map_WhenSuccess_TransformsPayload()
    {
        // Arrange
        var response = ApiResponse<int>.SuccessResponse(10, "message", "request-1");

        // Act
        var result = response.Map(i => i.ToString());

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("10");
        result.Message.Should().Be("message");
        result.RequestId.Should().Be("request-1");
    }

    [Fact]
    public void Map_WhenError_PropagatesError()
    {
        // Arrange
        var response = ApiResponse<int>.ErrorResponse("ERR_CODE", "Error message", "request-1");

        // Act
        var result = response.Map(i => i.ToString());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ERR_CODE");
        result.Message.Should().Be("Error message");
        result.RequestId.Should().Be("request-1");
    }
}
