using System.ComponentModel.DataAnnotations;
using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Builds a failure <see cref="Result"/> from <see cref="ValidationResult"/> items,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// Returns <see cref="Results.Success()"/> when <paramref name="errors"/> is null or empty.
    /// </summary>
    /// <param name="errors">Validation results to convert.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result ValidationFailure(
        IEnumerable<ValidationResult>? errors,
        ResultCode? code = null)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success()
            : Failure(ValidationMappingExtensions.ToMessagesPrefixed(list))
                .WithCode(code ?? ResultCodes.ValidationFailed);
    }

    /// <summary>
    /// Builds a failure <see cref="Result"/> from <see cref="ValidationResult"/> items using a custom enricher,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <param name="errors">Validation results to convert.</param>
    /// <param name="enrich">
    /// A delegate that receives the created <see cref="Message"/> and the chosen field name (if any),
    /// and returns the enriched <see cref="Message"/> (e.g., attach metadata).
    /// </param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result ValidationFailure(
        IEnumerable<ValidationResult>? errors,
        Func<Message, string?, Message> enrich,
        ResultCode? code = null)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success()
            : Failure(ValidationMappingExtensions.ToMessagesWithMetadata(list, enrich))
                .WithCode(code ?? ResultCodes.ValidationFailed);
    }

    /// <summary>
    /// Creates a validation failure <see cref="Result"/> from a single message,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <param name="reason">The validation failure reason.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    /// <param name="type">Message type (defaults to <see cref="MessageType.Error"/>).</param>
    /// <param name="metadata">Optional metadata for the message.</param>
    public static Result ValidationFailure(
        string reason = "Validation failed.",
        ResultCode? code = null,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Failure(reason, type, metadata)
            .WithCode(code ?? ResultCodes.ValidationFailed);

    /// <summary>
    /// Creates a validation failure <see cref="Result"/> from messages,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <param name="messages">Validation messages to include.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result ValidationFailure(
        IReadOnlyList<Message> messages,
        ResultCode? code = null)
        => Failure(messages)
            .WithCode(code ?? ResultCodes.ValidationFailed);

    /// <summary>
    /// Builds a failure <see cref="Result{T}"/> from <see cref="ValidationResult"/> items,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// Returns <see cref="Results.Success{T}()"/> when <paramref name="errors"/> is null or empty.
    /// </summary>
    /// <typeparam name="T">The (optional) value type for the result.</typeparam>
    /// <param name="errors">Validation results to convert.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result<T> ValidationFailure<T>(
        IEnumerable<ValidationResult>? errors,
        ResultCode? code = null)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success<T>()
            : Failure<T>(ValidationMappingExtensions.ToMessagesPrefixed(list))
                .WithCode(code ?? ResultCodes.ValidationFailed);
    }

    /// <summary>
    /// Builds a failure <see cref="Result{T}"/> from <see cref="ValidationResult"/> items using a custom enricher,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <typeparam name="T">The (optional) value type for the result.</typeparam>
    /// <param name="errors">Validation results to convert.</param>
    /// <param name="enrich">
    /// A delegate that receives the created <see cref="Message"/> and the chosen field name (if any),
    /// and returns the enriched <see cref="Message"/> (e.g., attach metadata).
    /// </param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result<T> ValidationFailure<T>(
        IEnumerable<ValidationResult>? errors,
        Func<Message, string?, Message> enrich,
        ResultCode? code = null)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Success<T>()
            : Failure<T>(ValidationMappingExtensions.ToMessagesWithMetadata(list, enrich))
                .WithCode(code ?? ResultCodes.ValidationFailed);
    }

    /// <summary>
    /// Creates a validation failure <see cref="Result{T}"/> from messages,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <typeparam name="T">The (optional) value type for the result.</typeparam>
    /// <param name="messages">Validation messages to include.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    public static Result<T> ValidationFailure<T>(
        IReadOnlyList<Message> messages,
        ResultCode? code = null)
        => Failure<T>(messages)
            .WithCode(code ?? ResultCodes.ValidationFailed);

    /// <summary>
    /// Creates a validation failure <see cref="Result{T}"/> from a single message,
    /// defaulting the domain code to <see cref="ResultCodes.ValidationFailed"/> (overridable).
    /// </summary>
    /// <typeparam name="T">The (optional) value type for the result.</typeparam>
    /// <param name="reason">The validation failure reason.</param>
    /// <param name="code">
    /// Optional domain code. Defaults to <see cref="ResultCodes.ValidationFailed"/> when not supplied.
    /// </param>
    /// <param name="type">Message type (defaults to <see cref="MessageType.Error"/>).</param>
    /// <param name="metadata">Optional metadata for the message.</param>
    public static Result<T> ValidationFailure<T>(
        string reason = "Validation failed.",
        ResultCode? code = null,
        MessageType type = MessageType.Error,
        IReadOnlyDictionary<string, object?>? metadata = null)
        => Failure<T>(reason, type, metadata)
            .WithCode(code ?? ResultCodes.ValidationFailed);

}
