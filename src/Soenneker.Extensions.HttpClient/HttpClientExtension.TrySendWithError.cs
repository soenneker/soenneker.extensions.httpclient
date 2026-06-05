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
    /// Attempts to execute send with error.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <typeparam name="TErrorResponse">The TErrorResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, string uri, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, uri);

        return await client.TrySendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Attempts to execute send with error.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <typeparam name="TErrorResponse">The TErrorResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="httpMethod">The http method.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="request">The request.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
    public static async ValueTask<(TSuccessResponse? SuccessResponse, TErrorResponse? ErrorResponse)> TrySendWithError<TSuccessResponse, TErrorResponse>(this System.Net.Http.HttpClient client, 
        HttpMethod httpMethod, string uri, object? request = null, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        using var requestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, uri);

        if (request != null)
            requestMessage.Content = request.TryToHttpContent();

        return await client.TrySendWithError<TSuccessResponse, TErrorResponse>(requestMessage, logger, cancellationToken).NoSync();
    }

    /// <summary>
    /// Attempts to execute send with error.
    /// </summary>
    /// <typeparam name="TSuccessResponse">The TSuccessResponse type.</typeparam>
    /// <typeparam name="TErrorResponse">The TErrorResponse type.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="request">The request.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the result of the operation.</returns>
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