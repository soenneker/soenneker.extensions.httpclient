using FluentAssertions;
using Soenneker.Extensions.HttpClient.Tests.Responses;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.HttpClientCache.Abstract;
using Xunit;
using Xunit.Abstractions;

namespace Soenneker.Extensions.HttpClient.Tests;

[Collection("Collection")]
public class HttpClientExtensionTests : FixturedUnitTest
{
    private readonly IHttpClientCache _cache;

    public HttpClientExtensionTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _cache = Resolve<IHttpClientCache>();
    }

    [Fact]
    public async System.Threading.Tasks.Task SendWithRetry_should_result()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests));

        var response = await client.SendWithRetryToType<TodoItemResponse>("https://jsonplaceholder.typicode.com/todos/1", logger: Logger);

        response.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task SendWithRetry_should_retry()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests));

        var response = await client.SendWithRetryToType<TodoItemResponse>("https://google.com", logger: Logger, log: false);

        response.Should().BeNull();
    }
}
