namespace Knight.Response.Models;

/// <summary>
/// Transport-agnostic domain code (e.g., "NotFound", "AlreadyExists", "InsufficientFunds").
/// Keep values stable so callers can map/log them.
/// </summary>
public sealed class ResultCode
{
    /// <summary>
    /// Create a new <see cref="ResultCode"/>.
    /// </summary>
    /// <param name="value">Non-empty domain code.</param>
    public ResultCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("ResultCode cannot be null or whitespace.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// The raw string value of the code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Implicitly construct a <see cref="ResultCode"/> from a string.
    /// </summary>
    public static implicit operator ResultCode(string value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value;
}
