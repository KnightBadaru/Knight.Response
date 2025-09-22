using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Models;

namespace Knight.Response.Mvc.Tests.Infrastructure;

public sealed class HyphenMapper : IValidationErrorMapper
{
    public Guid Id { get; } = Guid.NewGuid();
    public IDictionary<string, string[]> Map(IReadOnlyList<Message> _) =>
        new Dictionary<string, string[]> { ["_"] = new[] { Id.ToString() } };
}
