using Knight.Response.Core;
using Knight.Response.Abstractions.Http.Options;

namespace Knight.Response.Abstractions.Http.Resolution;

/// <summary>
/// Resolves the appropriate HTTP status code for <see cref="Result"/> instances
/// using configured delegates from <see cref="KnightResponseBaseOptions"/>.
/// </summary>
public static class ResultHttpResolver
{
    /// <summary>
    /// Resolves an HTTP status code for an untyped <see cref="Result"/>.
    /// </summary>
    /// <typeparam name="TH">HTTP context type of the host framework.</typeparam>
    /// <typeparam name="TP">ProblemDetails type of the host framework.</typeparam>
    /// <typeparam name="TV">ValidationProblemDetails type of the host framework.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="opts">
    /// Options containing <see cref="KnightResponseBaseOptions{TH,TP,TV}.CodeToHttp"/> and
    /// <see cref="KnightResponseBaseOptions{TH,TP,TV}.StatusCodeResolver"/>.
    /// </param>
    /// <returns>
    /// An HTTP status code determined by:
    /// <list type="number">
    ///   <item>
    ///     <description>
    ///       Invoking <see cref="KnightResponseBaseOptions{TH,TP,TV}.CodeToHttp"/> if set and a
    ///       <see cref="Result.Code"/> is present.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Falling back to <see cref="KnightResponseBaseOptions{TH,TP,TV}.StatusCodeResolver"/>
    ///       based on <see cref="Result.Status"/>.
    ///     </description>
    ///   </item>
    /// </list>
    /// </returns>
    public static int ResolveHttpCode<TH, TP, TV>(Result result, KnightResponseBaseOptions<TH, TP, TV> opts)
    {
        // 1. Try CodeToHttp
        var fromCode = opts.CodeToHttp?.Invoke(result.Code);
        if (fromCode.HasValue)
        {
            return fromCode.Value;
        }

        // 2. Try StatusCodeResolver if configured
        if (opts.StatusCodeResolver is not null)
        {
            return opts.StatusCodeResolver(result.Status);
        }

        // 3. Last resort: built-in defaults
        return KnightResponseHttpDefaults.StatusToHttp(result.Status);
    }

    /// <summary>
    /// Resolves an HTTP status code for a typed <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The carried value type.</typeparam>
    /// <typeparam name="TH">HTTP context type of the host framework.</typeparam>
    /// <typeparam name="TP">ProblemDetails type of the host framework.</typeparam>
    /// <typeparam name="TV">ValidationProblemDetails type of the host framework.</typeparam>
    /// <param name="result">The typed result to map.</param>
    /// <param name="opts">
    /// Options containing <see cref="KnightResponseBaseOptions{TH,TP,TV}.CodeToHttp"/> and
    /// <see cref="KnightResponseBaseOptions{TH,TP,TV}.StatusCodeResolver"/>.
    /// </param>
    /// <returns>
    /// The HTTP status code, resolved with the same precedence as the untyped overload.
    /// </returns>
    public static int ResolveHttpCode<T, TH, TP, TV>(Result<T> result, KnightResponseBaseOptions<TH, TP, TV> opts)
    {
        var fromCode = opts.CodeToHttp?.Invoke(result.Code);
        if (fromCode.HasValue)
        {
            return fromCode.Value;
        }

        if (opts.StatusCodeResolver is not null)
        {
            return opts.StatusCodeResolver(result.Status);
        }

        return KnightResponseHttpDefaults.StatusToHttp(result.Status);
    }
}
