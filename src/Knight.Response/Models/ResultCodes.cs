namespace Knight.Response.Models;

/// <summary>
/// Well-known, transport-agnostic <see cref="ResultCode"/> values.
/// These are optional conveniences—use your own domain codes freely.
/// </summary>
public static class ResultCodes
{
    /// <summary>
    /// Used when a requested resource does not exist.
    /// </summary>
    public static readonly ResultCode NotFound = new("NotFound");

    /// <summary>
    ///Used when a resource already exists and cannot be created again.
    ///</summary>
    public static readonly ResultCode AlreadyExists = new("AlreadyExists");

    /// <summary>
    ///Used when inputs fail validation.
    ///</summary>
    public static readonly ResultCode ValidationFailed = new("ValidationFailed");

    /// <summary>
    ///Used when the caller is not authenticated.
    ///</summary>
    public static readonly ResultCode Unauthorized = new("Unauthorized");

    /// <summary>
    ///Used when the caller is authenticated but not permitted.
    ///</summary>
    public static readonly ResultCode Forbidden = new("Forbidden");

    /// <summary>
    ///Used when a business precondition is not met (generic domain failure).
    ///</summary>
    public static readonly ResultCode PreconditionFailed = new("PreconditionFailed");

    /// <summary>
    ///Used for concurrency conflicts (e.g., ETag / rowversion mismatch).
    ///</summary>
    public static readonly ResultCode ConcurrencyConflict = new("ConcurrencyConflict");

    /// <summary>
    ///Used when the requested operation is not supported.
    ///</summary>
    public static readonly ResultCode NotSupported = new("NotSupported");

    /// <summary>
    ///Used when a dependency/service is unavailable.
    ///</summary>
    public static readonly ResultCode ServiceUnavailable = new("ServiceUnavailable");

    /// <summary>
    ///Used when an unexpected, unhandled error occurs.
    ///</summary>
    public static readonly ResultCode UnexpectedError = new("UnexpectedError");

    /// <summary>
    ///Used for successful creations.
    ///</summary>
    public static readonly ResultCode Created = new("Created");

    /// <summary>
    ///Used for successful updates.
    ///</summary>
    public static readonly ResultCode Updated = new("Updated");

    /// <summary>
    ///Used for successful deletions.
    ///</summary>
    public static readonly ResultCode Deleted = new("Deleted");

    /// <summary>
    /// Used when an operation completes successfully but there is no content to return.
    /// Typically maps to HTTP <c>204 No Content</c>.
    /// </summary>
    public static readonly ResultCode NoContent = new("NoContent");
}
