using Knight.Response.Core;
using Knight.Response.Extensions;
using Knight.Response.Models;

namespace Knight.Response.Factories;

/// <summary>
/// Factory methods for creating <see cref="Result"/> and <see cref="Result{T}"/> instances
/// with consistent statuses, messages, and (optionally) a domain <see cref="ResultCode"/>.
/// </summary>
public static partial class Results
{
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

    /// <summary>
    /// Creates a <see cref="Result"/> based on a boolean condition.
    /// </summary>
    public static Result FromCondition(bool condition, string errorMessage)
        => condition ? Success() : Failure(errorMessage);

    /// <summary>
    /// Creates a <see cref="Result{T}"/> based on a boolean condition.
    /// </summary>
    public static Result<T> FromCondition<T>(bool condition, T value, string errorMessage)
        => condition ? Success(value) : Failure<T>(errorMessage);
}
