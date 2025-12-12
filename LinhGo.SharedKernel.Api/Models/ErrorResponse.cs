namespace LinhGo.SharedKernel.Api.Models;

/// <summary>
/// Represents an API error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The type of error (e.g., NotFound, Validation, Conflict, Failure, Unexpected)
    /// </summary>
    /// <example>Validation</example>
    public required string Type { get; init; }

    /// <summary>
    /// List of detailed error information
    /// </summary>
    public required List<ErrorDetail> Errors { get; init; }

    /// <summary>
    /// Correlation ID for tracking the request across services
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public required string CorrelationId { get; init; }
}

/// <summary>
/// Detailed information about a specific error
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// The error code for programmatic handling
    /// </summary>
    /// <example>COMPANY_NOTFOUND</example>
    public required string Code { get; init; }

    /// <summary>
    /// Human-readable localized error message
    /// </summary>
    /// <example>Company with ID 42 not found</example>
    public required string Description { get; init; }
}