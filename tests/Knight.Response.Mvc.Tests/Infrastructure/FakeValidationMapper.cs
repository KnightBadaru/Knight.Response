using Knight.Response.Abstractions.Http.Mappers;
using Knight.Response.Models;

namespace Knight.Response.Mvc.Tests.Infrastructure;

public sealed class FakeValidationMapper : IValidationErrorMapper
{
    private readonly bool _yieldErrors;
    public FakeValidationMapper(bool yieldErrors = true) => _yieldErrors = yieldErrors;

    public IDictionary<string, string[]> Map(IReadOnlyList<Message> messages)
    {
        if (!_yieldErrors)
        {
            return new Dictionary<string, string[]>();
        }

        return new Dictionary<string, string[]>
        {
            ["name"] = messages.Select(m => m.Content).ToArray()
        };
    }
}
