using System;
using Soenneker.Extensions.Task;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Soenneker.Extensions.HttpResponseMessage;

namespace Soenneker.Extensions.HttpClient;

public static partial class HttpClientExtension
{
    /// <summary>
    /// Tries to send an HTTP GET request to the specified URI and retrieves the response content as a string, capturing any errors in the provided logger.
    /// </summary>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="uri">The URI to which the GET request will be sent.</param>
    /// <param name="logger">An optional <see cref="ILogger"/> instance for logging exceptions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a tuple:
    /// - <c>successful</c>: A boolean indicating whether the request succeeded.
    /// - <c>response</c>: The response content as a string, or <c>null</c> if the request failed.
    /// </returns>
    /// <remarks>
    /// If an exception occurs during the request, it is logged (if <paramref name="logger"/> is provided),
    /// and the method returns <c>(false, null)</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="uri"/> is null or empty.</exception>
    public static async ValueTask<(bool successful, string? response)> TrySendToString(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await client.TrySendToString(request, logger, cancellationToken);
    }

    /// <summary>
    /// Tries to send an HTTP request and retrieves the response content as a string, capturing any errors in the provided logger.
    /// </summary>
    /// <param name="client">The <see cref="System.Net.Http.HttpClient"/> instance used to send the request.</param>
    /// <param name="request">The <see cref="System.Net.Http.HttpRequestMessage"/> to send.</param>
    /// <param name="logger">An optional <see cref="ILogger"/> instance for logging exceptions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> containing a tuple:
    /// - <c>successful</c>: A boolean indicating whether the request succeeded.
    /// - <c>response</c>: The response content as a string, or <c>null</c> if the request failed.
    /// </returns>
    /// <remarks>
    /// If an exception occurs during the request, it is logged (if <paramref name="logger"/> is provided),
    /// and the method returns <c>(false, null)</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="client"/> or <paramref name="request"/> is null.</exception>
    public static async ValueTask<(bool successful, string? response)> TrySendToString(this System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using System.Net.Http.HttpResponseMessage response = await client.SendAsync(request, cancellationToken).NoSync();
            string result = await response.ToStringStrict(cancellationToken).NoSync();

            if (!response.IsSuccessStatusCode)
                logger?.LogError("HTTP request ({uri}) returned a non-successful status code ({statusCode})", request.RequestUri, response.StatusCode);

            return (response.IsSuccessStatusCode, result);
        }
        catch (Exception e)
        {
            logger?.LogError(e, "Exception sending request to {uri}", request.RequestUri);
            return (false, null);
        }
    }
}