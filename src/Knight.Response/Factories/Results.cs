using System.ComponentModel.DataAnnotations;
using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

/// <summary>
/// Factory methods for creating <see cref="Result"/> and <see cref="Result{T}"/> instances
/// with consistent statuses, messages, and (optionally) a domain <see cref="ResultCode"/>.
/// </summary>
public static class Results
{
    // -------- Success --------

    /// <summary>Creates a successful <see cref="Result"/>.</summary>
    public static Result Success() => new(Status.Completed);

    /// <summary>Creates a successful <see cref="Result"/> with an optional domain <see cref="ResultCode"/>.</summary>
    /// <param name="code">Domain reason identifier (e.g., "Created").</param>
    public static Result Success(ResultCode? code) => new(Status.Completed, code: code);

    /// <summary>Creates a successful <see cref="Result{T}"/> without a value.</summary>
    public static Result<T> Success<T>() => new(Status.Completed);

    /// <summary>Creates a successful <see cref="Result{T}"/> without a value, with an optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Success<T>(ResultCode? code) => new(Status.Completed, value: default, code: code);

    /// <summary>Creates a successful <see cref="Result{T}"/> with a value.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to return.</param>
    public static Result<T> Success<T>(T value) => new(Status.Completed, value);

    /// <summary>Creates a successful <see cref="Result{T}"/> with a value and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Success<T>(T value, ResultCode? code) => new(Status.Completed, value, code: code);

    /// <summary>Creates a successful <see cref="Result"/> with messages.</summary>
    /// <param name="messages">Additional messages to include.</param>
    public static Result Success(IReadOnlyList<Message> messages) => new(Status.Completed, messages: messages);

    /// <summary>Creates a successful <see cref="Result"/> with messages and an optional domain <see cref="ResultCode"/>.</summary>
    public static Result Success(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Completed, code: code, messages: messages);

    /// <summary>Creates a successful <see cref="Result{T}"/> with messages.</summary>
    public static Result<T> Success<T>(IReadOnlyList<Message> messages) => new(Status.Completed, value: default, messages: messages);

    /// <summary>Creates a successful <see cref="Result{T}"/> with messages and an optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Success<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Completed, value: default, code: code, messages: messages);

    /// <summary>Creates a successful <see cref="Result{T}"/> with value and messages.</summary>
    public static Result<T> Success<T>(T value, IReadOnlyList<Message> messages) => new(Status.Completed, value, messages: messages);

    /// <summary>Creates a successful <see cref="Result{T}"/> with value, messages, and an optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Success<T>(T value, IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Completed, value, code: code, messages: messages);


    // -------- Failure --------

    /// <summary>Creates a failed <see cref="Result"/> with messages.</summary>
    public static Result Failure(IReadOnlyList<Message> messages) => new(Status.Failed, messages: messages);

    /// <summary>Creates a failed <see cref="Result"/> with messages and an optional domain <see cref="ResultCode"/>.</summary>
    public static Result Failure(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Failed, code: code, messages: messages);

    /// <summary>Creates a failed <see cref="Result"/> with a single message.</summary>
    /// <param name="reason">The failure reason.</param>
    /// <param name="type">Message type for the reason (defaults to <see cref="MessageType.Error"/>).</param>
    /// <param name="metadata">Optional metadata.</param>
    public static Result Failure(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a failed <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result Failure(string reason, ResultCode? code, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a failed <see cref="Result{T}"/> with messages.</summary>
    public static Result<T> Failure<T>(IReadOnlyList<Message> messages) => new(Status.Failed, value: default, messages: messages);

    /// <summary>Creates a failed <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Failure<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Failed, value: default, code: code, messages: messages);

    /// <summary>Creates a failed <see cref="Result{T}"/> with a single message.</summary>
    public static Result<T> Failure<T>(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, value: default, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a failed <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Failure<T>(string reason, ResultCode? code, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Failed, value: default, code: code, messages: new List<Message> { new(type, reason, metadata) });


    // -------- Error --------

    /// <summary>Creates an error <see cref="Result"/> with messages.</summary>
    public static Result Error(IReadOnlyList<Message> messages) => new(Status.Error, messages: messages);

    /// <summary>Creates an error <see cref="Result"/> with messages and optional domain <see cref="ResultCode"/>.</summary>
    public static Result Error(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Error, code: code, messages: messages);

    /// <summary>Creates an error <see cref="Result"/> with a single message.</summary>
    public static Result Error(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates an error <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result Error(string reason, ResultCode? code, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates an error <see cref="Result{T}"/> with messages.</summary>
    public static Result<T> Error<T>(IReadOnlyList<Message> messages) => new(Status.Error, value: default, messages: messages);

    /// <summary>Creates an error <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Error<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Error, value: default, code: code, messages: messages);

    /// <summary>Creates an error <see cref="Result{T}"/> with a single message.</summary>
    public static Result<T> Error<T>(string reason, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, value: default, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates an error <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Error<T>(string reason, ResultCode? code, MessageType type = MessageType.Error, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Error, value: default, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Wraps an <see cref="Exception"/> as an error <see cref="Result"/>.</summary>
    public static Result Error(Exception ex) => Error(ex.Message);

    /// <summary>Wraps an <see cref="Exception"/> as an error <see cref="Result{T}"/>.</summary>
    public static Result<T> Error<T>(Exception ex) => Error<T>(ex.Message);


    // -------- Cancel --------

    /// <summary>Creates a cancelled <see cref="Result"/> with messages.</summary>
    public static Result Cancel(IReadOnlyList<Message> messages) => new(Status.Cancelled, messages: messages);

    /// <summary>Creates a cancelled <see cref="Result"/> with messages and optional domain <see cref="ResultCode"/>.</summary>
    public static Result Cancel(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Cancelled, code: code, messages: messages);

    /// <summary>Creates a cancelled <see cref="Result"/> with a single message (defaults to warning).</summary>
    public static Result Cancel(string reason, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a cancelled <see cref="Result"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result Cancel(string reason, ResultCode? code, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, code: code, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a cancelled <see cref="Result{T}"/> with messages.</summary>
    public static Result<T> Cancel<T>(IReadOnlyList<Message> messages) => new(Status.Cancelled, value: default, messages: messages);

    /// <summary>Creates a cancelled <see cref="Result{T}"/> with messages and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Cancel<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Cancelled, value: default, code: code, messages: messages);

    /// <summary>Creates a cancelled <see cref="Result{T}"/> with a single message (defaults to warning).</summary>
    public static Result<T> Cancel<T>(string reason, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, value: default, messages: new List<Message> { new(type, reason, metadata) });

    /// <summary>Creates a cancelled <see cref="Result{T}"/> with a single message and optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> Cancel<T>(string reason, ResultCode? code, MessageType type = MessageType.Warning, IReadOnlyDictionary<string, object?>? metadata = null)
        => new(Status.Cancelled, value: default, code: code, messages: new List<Message> { new(type, reason, metadata) });


    // -------- NotFound --------

    /// <summary>
    /// Creates a "not found" result. Defaults to <see cref="Status.Completed"/> with a <see cref="MessageType.Warning"/>.
    /// </summary>
    public static Result NotFound(string message = "Resource not found.", Status status = Status.Completed)
        => new(status, messages: new List<Message> { new(MessageType.Warning, message) });

    /// <summary>
    /// Creates a "not found" result with an optional domain <see cref="ResultCode"/> (e.g., "NotFound").
    /// </summary>
    public static Result NotFound(string message, ResultCode? code, Status status = Status.Completed)
        => new(status, code: code, messages: new List<Message> { new(MessageType.Warning, message) });

    /// <summary>Creates a typed "not found" result. Defaults to Completed + Warning.</summary>
    public static Result<T> NotFound<T>(string message = "Resource not found.", Status status = Status.Completed)
        => new(status, value: default, messages: new List<Message> { new(MessageType.Warning, message) });

    /// <summary>Creates a typed "not found" result with optional domain <see cref="ResultCode"/>.</summary>
    public static Result<T> NotFound<T>(string message, ResultCode? code, Status status = Status.Completed)
        => new(status, value: default, code: code, messages: new List<Message> { new(MessageType.Warning, message) });


    // -------- Validation --------

    /// <summary>
    /// Builds a <see cref="Result"/> from <see cref="ValidationResult"/> items.
    /// Returns <see cref="Results.Success()"/> when <paramref name="errors"/> is null or empty.
    /// </summary>
    public static Result Validation(IEnumerable<ValidationResult>? errors)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success()
            : Error(ValidationMappingExtensions.ToMessagesPrefixed(list));
    }

    /// <summary>
    /// Builds a <see cref="Result"/> from <see cref="ValidationResult"/> items, with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Validation(IEnumerable<ValidationResult>? errors, ResultCode? code)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success(code)
            : Error(ValidationMappingExtensions.ToMessagesPrefixed(list), code);
    }

    /// <summary>
    /// Builds a <see cref="Result{T}"/> from <see cref="ValidationResult"/> items.
    /// Returns <see cref="Results.Success{T}()"/> when <paramref name="errors"/> is null or empty.
    /// </summary>
    public static Result<T> Validation<T>(IEnumerable<ValidationResult>? errors)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success<T>()
            : Error<T>(ValidationMappingExtensions.ToMessagesPrefixed(list));
    }

    /// <summary>
    /// Builds a <see cref="Result{T}"/> from <see cref="ValidationResult"/> items, with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Validation<T>(IEnumerable<ValidationResult>? errors, ResultCode? code)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success<T>(code)
            : Error<T>(ValidationMappingExtensions.ToMessagesPrefixed(list), code);
    }

    /// <summary>
    /// Builds a <see cref="Result"/> from <see cref="ValidationResult"/> items using a custom enricher.
    /// </summary>
    public static Result Validation(
        IEnumerable<ValidationResult>? errors,
        Func<Message, string?, Message> enrich)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success()
            : Error(ValidationMappingExtensions.ToMessagesWithMetadata(list, enrich));
    }

    /// <summary>
    /// Builds a <see cref="Result{T}"/> from <see cref="ValidationResult"/> items using a custom enricher.
    /// </summary>
    public static Result<T> Validation<T>(
        IEnumerable<ValidationResult>? errors,
        Func<Message, string?, Message> enrich)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success<T>()
            : Error<T>(ValidationMappingExtensions.ToMessagesWithMetadata(list, enrich));
    }


    // -------- Aggregate / FromCondition --------

    /// <summary>
    /// Aggregates multiple results into a single result. Returns success if all are successful,
    /// otherwise returns a failure with the combined messages.
    /// </summary>
    public static Result Aggregate(IEnumerable<Result> results)
    {
        var resultList = results is IList<Result> r ? r : new List<Result>(results);
        var failed = new List<Message>();

        foreach (var result in resultList)
        {
            if (!result.IsSuccess())
            {
                foreach (var message in result.Messages)
                {
                    failed.Add(message);
                }
            }
        }

        return failed.Count == 0 ? Success() : Failure(failed);
    }

    /// <summary>Creates a <see cref="Result"/> based on a boolean condition.</summary>
    public static Result FromCondition(bool condition, string errorMessage)
        => condition ? Success() : Failure(errorMessage);

    /// <summary>Creates a <see cref="Result{T}"/> based on a boolean condition.</summary>
    public static Result<T> FromCondition<T>(bool condition, T value, string errorMessage)
        => condition ? Success(value) : Failure<T>(errorMessage);
}
