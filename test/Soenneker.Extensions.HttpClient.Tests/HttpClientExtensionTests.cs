using FluentAssertions;
using Soenneker.Extensions.HttpClient.Tests.Responses;
using Soenneker.Tests.FixturedUnit;
using Soenneker.Utils.HttpClientCache.Abstract;
using Xunit;

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
    public async System.Threading.Tasks.Task SendWithRetryToType_should_result()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests), cancellationToken: CancellationToken);

        var response = await client.SendWithRetryToType<TodoItemResponse>("https://jsonplaceholder.typicode.com/todos/1", logger: Logger, cancellationToken: CancellationToken);

        response.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task TrySendWithRetryToType_should_return_null()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests), cancellationToken: CancellationToken);

        var response = await client.TrySendWithRetryToType<TodoItemResponse>("https://google.com", logger: Logger, log: false, cancellationToken: CancellationToken);

        response.Should().BeNull();
    }
}