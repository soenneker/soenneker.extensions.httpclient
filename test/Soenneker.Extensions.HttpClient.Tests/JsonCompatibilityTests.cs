using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using AwesomeAssertions;

namespace Soenneker.Extensions.HttpClient.Tests;

public class JsonCompatibilityTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async System.Threading.Tasks.Task Object_request_and_typed_response_preserve_web_serialization(bool trySend)
    {
        using var handler = new JsonResponseHandler(HttpStatusCode.OK);
        using var client = new System.Net.Http.HttpClient(handler);
        object request = new Payload { DisplayName = "request" };

        Payload? response = trySend
            ? await client.TrySendToType<Payload>(HttpMethod.Post, "https://example.test/payload", request)
            : await client.SendToType<Payload>(HttpMethod.Post, "https://example.test/payload", request);

        response.Should().NotBeNull();
        response!.DisplayName.Should().Be("response");
        using JsonDocument body = JsonDocument.Parse(handler.RequestBody!);
        body.RootElement.GetProperty("displayName").GetString().Should().Be("request");
        handler.ContentType.Should().Be("application/json");
    }

    [Test]
    public async System.Threading.Tasks.Task Typed_error_response_is_deserialized()
    {
        using var client = new System.Net.Http.HttpClient(new JsonResponseHandler(HttpStatusCode.BadRequest));

        var (success, error) = await client.SendWithError<Payload, Payload>("https://example.test/payload");

        success.Should().BeNull();
        error.Should().NotBeNull();
        error!.DisplayName.Should().Be("response");
    }

    public sealed class Payload
    {
        public string? DisplayName { get; set; }
    }

    private sealed class JsonResponseHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public string? RequestBody { get; private set; }
        public string? ContentType { get; private set; }

        protected override async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                ContentType = request.Content.Headers.ContentType?.MediaType;
            }

            return new System.Net.Http.HttpResponseMessage(statusCode)
            {
                Content = new StringContent("{\"displayName\":\"response\"}", Encoding.UTF8, "application/json")
            };
        }
    }
}
