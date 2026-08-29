using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Object;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.Extensions.HttpClient;

/// <summary>
/// Represents the http client extension.
/// </summary>
public static partial class HttpClientExtension
{
    /// <summary>
    /// Attempts to send an HTTP request and returns whichever typed success or error payload can be read without propagating conversion failures.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <typeparam name="TErrorResponse">The error payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="uri">The destination URI.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple in which the payload matching the HTTP outcome is populated and the other value is null.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return await client.TrySendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Attempts to send an HTTP request and returns whichever typed success or error payload can be read without propagating conversion failures.
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
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await client.TrySendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Attempts to send an HTTP request and returns whichever typed success or error payload can be read without propagating conversion failures.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The success payload type.</typeparam>
    /// <typeparam name="TErrorResponse">The error payload type.</typeparam>
    /// <param name="client">The HTTP client used to send the request.</param>
    /// <param name="request">The request payload, or a prepared request message for the matching overload.</param>
    /// <param name="logger">An optional logger for request and conversion failures.</param>
    /// <param name="cancellationToken">Signals that the asynchronous operation should stop.</param>
    /// <returns>A tuple in which the payload matching the HTTP outcome is populated and the other value is null.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        System.Net.Http.HttpRequestMessage request, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await client.SendWithError<TSuccessResponse?, TErrorResponse?>(request, logger, cancellationToken).NoSync();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Exhausted all retry attempts for the HTTP request, returning null");
            return default;
        }
    }
}