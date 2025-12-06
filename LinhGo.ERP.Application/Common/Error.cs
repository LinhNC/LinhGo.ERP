namespace LinhGo.ERP.Application.Common;

/// <summary>
/// Represents an error with a code, description, type, and parameters.
/// </summary>
public readonly record struct Error
{
    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error description (used for custom message).
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the parameters for localized message formatting.
    /// </summary>
    public object[]? Parameters { get; }

    /// <summary>
    /// Gets the error type.
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Gets the numeric value of the type.
    /// </summary>
    public int NumericType { get; }
    
    private Error(string code, ErrorType type)
    {
        Code = code;
        Type = type;
        NumericType = (int)type;
        Parameters = null;
    }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
        NumericType = (int)type;
        Parameters = null;
    }

    private Error(string code, ErrorType type, params object[] parameters)
    {
        Code = code;
        Type = type;
        NumericType = (int)type;
        Parameters = parameters?.Length > 0 ? parameters : null;
    }

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    public static Error WithFailureCode(string code) =>
        new(code, ErrorType.Failure);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Failure"/> from a code with parameters.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="parameters">Parameters for message formatting.</param>
    public static Error WithFailureCode(string code, params object[] parameters) =>
        new(code, ErrorType.Failure, parameters);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Unexpected"/> from a code.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    public static Error WithUnexpectedCode(string code) =>
        new(code, ErrorType.Unexpected);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Unexpected"/> from a code with parameters.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="parameters">Parameters for message formatting.</param>
    public static Error WithUnexpectedCode(string code, params object[] parameters) =>
        new(code, ErrorType.Unexpected, parameters);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Validation"/> from a code.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    public static Error WithValidationCode(string code) =>
        new(code, ErrorType.Validation);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Validation"/> from a code with parameters.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="parameters">Parameters for message formatting.</param>
    public static Error WithValidationCode(string code, params object[] parameters) =>
        new(code, ErrorType.Validation, parameters);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Conflict"/> from a code.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    public static Error WithConflictCode(string code) =>
        new(code, ErrorType.Conflict);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.Conflict"/> from a code with parameters.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="parameters">Parameters for message formatting.</param>
    public static Error WithConflictCode(string code, params object[] parameters) =>
        new(code, ErrorType.Conflict, parameters);
    

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.NotFound"/> from a code.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    public static Error WithNotFoundCode(string code) =>
        new(code, ErrorType.NotFound);

    /// <summary>
    /// Creates an <see cref="Error"/> of type <see cref="ErrorType.NotFound"/> from a code with parameters.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="parameters">Parameters for message formatting.</param>
    public static Error WithNotFoundCode(string code, params object[] parameters) =>
        new(code, ErrorType.NotFound, parameters);

    /// <summary>
    /// Creates an <see cref="Error"/> with the given numeric <paramref name="type"/>,
    /// <paramref name="code"/>, and <paramref name="description"/>.
    /// </summary>
    /// <param name="type">An integer value which represents the type of error that occurred.</param>
    /// <param name="code">The unique error code.</param>
    /// <param name="description">The error description.</param>
    public static Error WithCustomMessage(
        int type,
        string code,
        string description) =>
        new(code, description, (ErrorType)type);

    public override string ToString()
    {
        return $"{Code}:{Description}";
    }
}

/// <summary>
/// Error types.
/// </summary>
public enum ErrorType
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
}
