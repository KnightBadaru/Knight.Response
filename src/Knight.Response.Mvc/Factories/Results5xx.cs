namespace Knight.Response.Mvc.Factories;

/// <summary>
/// Provides static helpers to convert <see cref="Result"/> and <see cref="Result{T}"/>
/// into <see cref="IActionResult"/> for ASP.NET Core MVC applications.
/// </summary>
/// <remarks>
/// These helpers honor <see cref="KnightResponseOptions"/> when a <see cref="HttpContext"/> is available
/// (payload shape, ProblemDetails behavior, etc.). Defaults are used when no context is provided.
/// </remarks>
public static partial class ApiResults
{

}
