using AwesomeAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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

    [Test]
    public async System.Threading.Tasks.Task SendToType_rejects_a_non_success_response_even_when_the_body_matches()
    {
        using var client = new System.Net.Http.HttpClient(new StaticResponseHandler());

        await Assert.That(async () => await client.SendToType<TodoItemResponse>("https://example.test/todo"))
                    .Throws<HttpRequestException>();
    }

    private sealed class StaticResponseHandler : System.Net.Http.HttpMessageHandler
    {
        protected override System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new System.Net.Http.HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"userId\":1,\"id\":1,\"title\":\"error\",\"completed\":false}", Encoding.UTF8,
                    "application/json")
            };

            return System.Threading.Tasks.Task.FromResult(response);
        }
    }
}
