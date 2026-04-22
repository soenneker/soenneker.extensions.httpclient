using AwesomeAssertions;
using Soenneker.Extensions.HttpClient.Tests.Responses;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.HttpClientCache.Abstract;

namespace Soenneker.Extensions.HttpClient.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class HttpClientExtensionTests : HostedUnitTest
{
    private readonly IHttpClientCache _cache;

    public HttpClientExtensionTests(Host host) : base(host)
    {
        _cache = Resolve<IHttpClientCache>();
    }

    [Test]
    public async System.Threading.Tasks.Task SendToTypeWithRetry_should_result()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests), cancellationToken: System.Threading.CancellationToken.None);

        var response = await client.SendToTypeWithRetry<TodoItemResponse>("https://jsonplaceholder.typicode.com/todos/1", logger: Logger, cancellationToken: System.Threading.CancellationToken.None);

        response.Should().NotBeNull();
    }

    [Test]
    public async System.Threading.Tasks.Task TrySendToTypeWithRetry_should_return_null()
    {
        System.Net.Http.HttpClient client = await _cache.Get(nameof(HttpClientExtensionTests), cancellationToken: System.Threading.CancellationToken.None);

        var response = await client.TrySendToTypeWithRetry<TodoItemResponse>("https://google.com", logger: Logger, log: false, cancellationToken: System.Threading.CancellationToken.None);

        response.Should().BeNull();
    }
}
