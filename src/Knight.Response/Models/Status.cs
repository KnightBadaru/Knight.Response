using System.Text.Json.Serialization;

namespace Knight.Response.Models;

/// <summary>
/// Represents the overall outcome of an operation or process.
/// </summary>
/// <remarks>
/// This status is typically returned by API methods to indicate whether the
/// operation completed successfully, was cancelled, failed due to a known issue,
/// or encountered an unexpected error.
/// </remarks>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    /// <summary>
    /// The operation completed successfully without errors.
    /// </summary>
    Completed,

    /// <summary>
    /// The operation was intentionally cancelled before it could complete.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The operation failed due to a known or expected condition.
    /// </summary>
    Failed,

    /// <summary>
    /// The operation encountered an unexpected or unhandled error.
    /// </summary>
    Error
}
