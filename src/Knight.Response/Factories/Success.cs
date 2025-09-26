using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Factories;

public static partial class Results
{
    /// <summary>
    /// Creates a successful <see cref="Result"/>.
    /// </summary>
    public static Result Success() => new(Status.Completed);

    /// <summary>
    /// Creates a successful <see cref="Result"/> with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    /// <param name="code">
    /// Domain reason identifier (e.g., "Created").
    /// </param>
    public static Result Success(ResultCode? code) => new(Status.Completed, code: code);


    // -------- Typed --------

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> without a value.
    /// </summary>
    public static Result<T> Success<T>() => new(Status.Completed);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> without a value, with an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Success<T>(ResultCode? code) => new(Status.Completed, code: code);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to return.</param>
    public static Result<T> Success<T>(T value) => new(Status.Completed, value);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with a value and optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Success<T>(T value, ResultCode? code) => new(Status.Completed, value, code: code);

    /// <summary>
    /// Creates a successful <see cref="Result"/> with messages.
    /// </summary>
    /// <param name="messages">Additional messages to include.</param>
    public static Result Success(IReadOnlyList<Message> messages) => new(Status.Completed, messages: messages);

    /// <summary>
    /// Creates a successful <see cref="Result"/> with messages and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result Success(IReadOnlyList<Message> messages, ResultCode? code) => new(Status.Completed, code: code, messages: messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with messages.
    /// </summary>
    public static Result<T> Success<T>(IReadOnlyList<Message> messages) => new(Status.Completed, messages: messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with messages and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Success<T>(IReadOnlyList<Message> messages, ResultCode? code)
        => new(Status.Completed, value: default, code: code, messages: messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with value and messages.
    /// </summary>
    public static Result<T> Success<T>(T value, IReadOnlyList<Message> messages) => new(Status.Completed, value, messages: messages);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> with value, messages, and an optional domain <see cref="ResultCode"/>.
    /// </summary>
    public static Result<T> Success<T>(
        T value,
        IReadOnlyList<Message> messages,
        ResultCode? code
        )
        => new(Status.Completed, value, code: code, messages: messages);
}
