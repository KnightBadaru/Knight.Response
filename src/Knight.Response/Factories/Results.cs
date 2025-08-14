using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

/// <summary>
/// Factory methods for creating <see cref="Result"/> and <see cref="Result{T}"/> instances
/// with consistent statuses and messages.
/// </summary>
public static class Results
{
    // -------- Success --------

    /// <summary>
    /// Creates a successful <see cref="Result"/>.
    /// </summary>
    public static Result Success() => new(Status.Completed);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> without a value.
    /// </summary>
    public static Result<T> Success<T>() => new(Status.Completed);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to return.</param>
    public static Result<T> Success<T>(T value) => new(Status.Completed, value);

    /// <summary>
    /// Creates a successful <see cref="Result"/> with messages.
    /// </summary>
    /// <param name="messages">Additional messages to include.</param>
    public static Result Success(IReadOnlyList<Message> messages) => new(Status.Completed, messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with messages.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="messages">Additional messages to include.</param>
    public static Result<T> Success<T>(IReadOnlyList<Message> messages) => new(Status.Completed, default, messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with value and messages.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to return.</param>
    /// <param name="messages">Additional messages to include.</param>
    public static Result<T> Success<T>(T value, IReadOnlyList<Message> messages) => new(Status.Completed, value, messages);

    // -------- Failure --------

    /// <summary>
    /// Creates a failed <see cref="Result"/> with messages.
    /// </summary>
    public static Result Failure(IReadOnlyList<Message> messages) => new(Status.Failed, messages);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with a single message.
    /// </summary>
    /// <param name="reason">The failure reason.</param>
    /// <param name="type">Message type for the reason (defaults to <see cref="MessageType.Error"/>).</param>
    /// <param name="metadata">Optional metadata.</param>
    public static Result Failure(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Failure<T>(IReadOnlyList<Message> messages) => new(Status.Failed, default, messages);

    /// <summary>
    /// Creates a failed <see cref="Result{T}"/> with a single message.
    /// </summary>
    public static Result<T> Failure<T>(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, default, new List<Message> { new(type, reason, metadata) });

    // -------- Error --------

    /// <summary>
    /// Creates an error <see cref="Result"/> with messages.
    /// </summary>
    public static Result Error(IReadOnlyList<Message> messages) => new(Status.Error, messages);

    /// <summary>
    /// Creates an error <see cref="Result"/> with a single message.
    /// </summary>
    public static Result Error(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Error<T>(IReadOnlyList<Message> messages) => new(Status.Error, default, messages);

    /// <summary>
    /// Creates an error <see cref="Result{T}"/> with a single message.
    /// </summary>
    public static Result<T> Error<T>(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, default, new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Wraps an <see cref="Exception"/> as an error <see cref="Result"/>.
    /// </summary>
    public static Result Error(Exception ex) => Error(ex.Message);

    /// <summary>
    /// Wraps an <see cref="Exception"/> as an error <see cref="Result{T}"/>.
    /// </summary>
    public static Result<T> Error<T>(Exception ex) => Error<T>(ex.Message);

    // -------- Cancel --------

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with messages.
    /// </summary>
    public static Result Cancel(IReadOnlyList<Message> messages) => new(Status.Cancelled, messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result"/> with a single message (defaults to warning).
    /// </summary>
    public static Result Cancel(string reason, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, new List<Message> { new(type, reason, metadata) });

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Cancel<T>(IReadOnlyList<Message> messages) => new(Status.Cancelled, default, messages);

    /// <summary>
    /// Creates a cancelled <see cref="Result{T}"/> with a single message (defaults to warning).
    /// </summary>
    public static Result<T> Cancel<T>(string reason, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, default, new List<Message> { new(type, reason, metadata) });

    // -------- NotFound --------

    /// <summary>
    /// Creates a "not found" result. Defaults to <see cref="Status.Completed"/> with a <see cref="MessageType.Warning"/>.
    /// </summary>
    /// <param name="message">The description (e.g. "User not found").</param>
    /// <param name="status">
    /// Optional override status. Use <see cref="Status.Failed"/> if "not found" is a business failure,
    /// or <see cref="Status.Cancelled"/> when a lookup was aborted.
    /// </param>
    public static Result NotFound(string message = "Resource not found.", Status status = Status.Completed)
        => new(status, new List<Message> { new(MessageType.Warning, message) });

    /// <summary>
    /// Creates a typed "not found" result. Defaults to <see cref="Status.Completed"/> with a <see cref="MessageType.Warning"/>.
    /// </summary>
    public static Result<T> NotFound<T>(string message = "Resource not found.", Status status = Status.Completed)
        => new(status, default, new List<Message> { new(MessageType.Warning, message) });

    // -------- Aggregate / FromCondition --------

    /// <summary>
    /// Aggregates multiple results into a single result. Returns success if all are successful,
    /// otherwise returns a failure with the combined messages.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    public static Result Aggregate(IEnumerable<Result> results)
    {
        var resultList = results is IList<Result> r ? r : new List<Result>(results);
        var failed = new List<Message>();

        foreach (var result in resultList)
        {
            if (!result.IsSuccess)
            {
                foreach (var message in result.Messages)
                {
                    failed.Add(message);
                }
            }
        }

        return failed.Count == 0 ? Success() : Failure(failed);
    }

    /// <summary>
    /// Creates a <see cref="Result"/> based on a condition.
    /// </summary>
    /// <param name="condition">If true, returns success; otherwise failure with the provided message.</param>
    /// <param name="errorMessage">The message to include when the condition is false.</param>
    public static Result FromCondition(bool condition, string errorMessage)
        => condition ? Success() : Failure(errorMessage);

    /// <summary>
    /// Creates a <see cref="Result{T}"/> based on a condition.
    /// </summary>
    /// <param name="condition">If true, returns success with the provided value; otherwise failure with the provided message.</param>
    /// <param name="value">The value to include when the condition is true.</param>
    /// <param name="errorMessage">The message to include when the condition is false.</param>
    public static Result<T> FromCondition<T>(bool condition, T value, string errorMessage)
        => condition ? Success(value) : Failure<T>(errorMessage);
}
