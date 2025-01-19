using System;
using System.Net.Http;
using Soenneker.Extensions.Task;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.ValueTask;
using Soenneker.Extensions.HttpResponseMessage;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP GET request to the specified URI and retrieves the response content as a string.
    /// </summary>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="uri">The URI to which the GET request will be sent.</param>
    /// <param name="logger">An optional <see cref="ILogger"/> instance for logging errors or additional information.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the response content as a string.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="uri"/> is null or empty.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    /// <exception cref="TaskCanceledException">Thrown if the request is canceled via the <paramref name="cancellationToken"/>.</exception>

    public static async ValueTask<string> SendToString(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return await client.SendToString(request, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request and retrieves the response content as a string.
    /// </summary>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="request">The <see cref="System.Net.Http.HttpRequestMessage"/> to send.</param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing the response content as a string.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown if the HTTP request fails.</exception>
    /// <exception cref="TaskCanceledException">Thrown if the request is canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async ValueTask<string> SendToString(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        using System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();

        if (!response.IsSuccessStatusCode)
            logger?.LogError("HTTP request ({uri}) returned a non-successful status code ({statusCode})", request.RequestUri, response.StatusCode);

        return await response.ToStringStrict(cancellationToken).NoSync();
    }
}