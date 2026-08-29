using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.HttpResponseMessage;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// Represents the http client extension.
/// </summary>
public static partial class HttpClientExtension
{
    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or the typed error payload according to the status code.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <typeparam name="TErrorResponse">The error payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple in which the payload matching the HTTP outcome is populated and the other value is null.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);
        return await client.SendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or the typed error payload according to the status code.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <typeparam name="TErrorResponse">The error payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="httpMethod">The HTTP method used when constructing the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="request">The request payload, or a prepared request message for the matching overload.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple in which the payload matching the HTTP outcome is populated and the other value is null.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.ToHttpContent();

        return await client.SendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Sends an HTTP request and deserializes either the success payload or the typed error payload according to the status code.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <typeparam name="TErrorResponse">The error payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="requestMessage">The prepared HTTP request message; ownership follows <see cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple in which the payload matching the HTTP outcome is populated and the other value is null.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> SendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        System.Net.Http.HttpRequestMessage requestMessage, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using System.Net.Http.HttpResponseMessage response = await client.SendAsync(requestMessage, cancellationToken).NoSync();

        if (!response.IsSuccessStatusCode)
        {
            TErrorResponse? errorResponse = await response.To<TErrorResponse>(logger, cancellationToken).NoSync();
            return (default, errorResponse);
        }

        TSuccessResponse? successResponse = await response.To<TSuccessResponse>(logger, cancellationToken).NoSync();
        return (successResponse, default);
    }
}