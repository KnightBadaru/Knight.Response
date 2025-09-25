using Knight.Response.Core;
using Knight.Response.Models;

namespace Knight.Response.Abstractions.Http.Resolution;

/// <summary>
/// Opt-in HTTP mapping defaults. Use these delegates directly or copy them and tweak per project.
/// </summary>
public static class KnightResponseHttpDefaults
{
    /// <summary>
    /// Optional mapping from well-known <see cref="ResultCodes"/> to HTTP status codes.
    /// Returns <c>null</c> for unknown codes so the resolver can fall back to status mapping.
    /// </summary>
    public static readonly Func<ResultCode?, int?> CodeToHttp = code =>
    {
        if (code is null)
        {
            return null;
        }

        return code.Value switch
        {
            // success/no-content
            var v when v == ResultCodes.Created.Value             => 201,
            var v when v == ResultCodes.Updated.Value             => 200,
            var v when v == ResultCodes.Deleted.Value             => 200,
            var v when v == ResultCodes.NoContent.Value           => 204,

            // client issues
            var v when v == ResultCodes.ValidationFailed.Value    => 400,
            var v when v == ResultCodes.Unauthorized.Value        => 401,
            var v when v == ResultCodes.Forbidden.Value           => 403,
            var v when v == ResultCodes.NotFound.Value            => 404,
            var v when v == ResultCodes.AlreadyExists.Value       => 409,
            var v when v == ResultCodes.PreconditionFailed.Value  => 412,
            var v when v == ResultCodes.ConcurrencyConflict.Value => 409,

            // server issues
            var v when v == ResultCodes.ServiceUnavailable.Value  => 503,
            var v when v == ResultCodes.UnexpectedError.Value     => 500,

            _ => (int?)null
        };
    };

    /// <summary>
    /// Default fallback mapping from <see cref="Status"/> to HTTP codes.
    /// Mirrors the library’s built-in default.
    /// </summary>
    public static readonly Func<Status, int> StatusToHttp = s =>
        s == Status.Error     ? 500:
        s == Status.Cancelled ? 409 :
        s == Status.Failed    ? 400 :
                                200;
}
