using Microsoft.AspNetCore.Http;

namespace Knight.Response.Mvc.Tests.Infrastructure;

public record TestResult(int Status, string Body, IHeaderDictionary Headers);
